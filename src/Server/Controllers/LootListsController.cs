// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class LootListsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;
        private readonly IAuthorizationService _authorizationService;
        private readonly TelemetryClient _telemetry;

        public LootListsController(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo, IAuthorizationService authorizationService, TelemetryClient telemetry)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
            _authorizationService = authorizationService;
            _telemetry = telemetry;
        }

        [HttpGet]
        public async Task<ActionResult<IList<LootListDto>>> Get(long? characterId = null, long? teamId = null, byte? phase = null)
        {
            try
            {
                var lootLists = await CreateDtosAsync(characterId, teamId, phase);

                if (lootLists is null)
                {
                    return NotFound();
                }

                return Ok(lootLists);
            }
            catch (ArgumentException ex)
            {
                return Problem(ex.Message, statusCode: 400);
            }
        }

        [HttpPost("Phase{phase:int}/{characterId:long}")]
        public async Task<ActionResult<LootListDto>> PostLootList(long characterId, byte phase, [FromBody] LootListSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, characterId, AppPolicies.CharacterOwnerOrAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var bracketTemplates = await _context.Brackets.AsNoTracking().Where(b => b.Phase == phase).OrderBy(b => b.Index).ToListAsync();

            if (bracketTemplates.Count == 0)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            Debug.Assert(dto.MainSpec.HasValue);

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
            {
                return Problem("A loot list for that character and phase already exists.");
            }

            if (!dto.MainSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec.HasValue && !dto.OffSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            var list = new CharacterLootList
            {
                CharacterId = character.Id,
                Character = character,
                Entries = new List<LootListEntry>(28),
                Status = LootListStatus.Editing,
                MainSpec = dto.MainSpec.Value,
                OffSpec = dto.OffSpec ?? dto.MainSpec.Value,
                Phase = phase
            };

            var entries = new List<LootListEntry>();

            var brackets = await _context.Brackets
                .AsNoTracking()
                .Where(b => b.Phase == phase)
                .OrderBy(b => b.Index)
                .ToListAsync();

            foreach (var bracket in brackets)
            {
                for (byte rank = bracket.MinRank; rank <= bracket.MaxRank; rank++)
                {
                    for (int col = 0; col < bracket.MaxItems; col++)
                    {
                        var entry = new LootListEntry(idGenerator.CreateId())
                        {
                            LootList = list,
                            Rank = rank
                        };
                        _context.LootListEntries.Add(entry);
                        entries.Add(entry);
                    }
                }
            }

            _context.CharacterLootLists.Add(list);

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListCreated", User, props =>
            {
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
                props["Phase"] = list.Phase.ToString();
            });

            var scope = PrioCalculator.Scope;
            var attendance = await _context.RaidAttendees
                .AsNoTracking()
                .Where(x => !x.IgnoreAttendance && x.CharacterId == character.Id && x.Raid.RaidTeamId == character.TeamId)
                .Select(x => x.Raid.StartedAt.Date)
                .Distinct()
                .OrderByDescending(x => x)
                .Take(scope.ObservedAttendances)
                .CountAsync();

            var now = _serverTimeZoneInfo.TimeZoneNow();

            var donations = await _context.Donations
                .AsNoTracking()
                .Where(d => d.CharacterId == character.Id && d.Month == now.Month && d.Year == now.Year)
                .SumAsync(d => (long)d.CopperAmount);

            var returnDto = new LootListDto
            {
                ApprovedBy = null,
                Bonuses = PrioCalculator.GetListBonuses(PrioCalculator.Scope, attendance, character.MemberStatus, donations).ToList(),
                CharacterId = character.Id,
                CharacterMemberStatus = character.MemberStatus,
                CharacterName = character.Name,
                MainSpec = list.MainSpec,
                OffSpec = list.OffSpec,
                Phase = list.Phase,
                Status = list.Status,
                TeamId = character.TeamId,
                SubmittedToId = list.SubmittedToId,
                Timestamp = list.Timestamp
            };

            if (returnDto.TeamId.HasValue)
            {
                returnDto.TeamName = await _context.RaidTeams.AsNoTracking().Where(t => t.Id == returnDto.TeamId).Select(t => t.Name).FirstOrDefaultAsync();
            }
            if (returnDto.SubmittedToId.HasValue)
            {
                if (returnDto.SubmittedToId == returnDto.TeamId)
                {
                    returnDto.SubmittedToName = returnDto.TeamName;
                }
                else
                {
                    returnDto.SubmittedToName = await _context.RaidTeams.AsNoTracking().Where(t => t.Id == returnDto.SubmittedToId).Select(t => t.Name).FirstOrDefaultAsync();
                }
            }

            foreach (var entry in entries)
            {
                var bracket = brackets.Find(b => entry.Rank >= b.MinRank && entry.Rank <= b.MaxRank);
                Debug.Assert(bracket is not null);
                returnDto.Entries.Add(new LootListEntryDto
                {
                    Bracket = bracket.Index,
                    BracketAllowsOffspec = bracket.AllowOffspec,
                    BracketAllowsTypeDuplicates = bracket.AllowTypeDuplicates,
                    Id = entry.Id,
                    Rank = entry.Rank
                });
            }

            return CreatedAtAction(nameof(Get), new { characterId = character.Id, phase }, returnDto);
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/Reset")]
        public async Task<ActionResult<LootListDto>> ResetLootList(long characterId, byte phase, [FromServices] IdGen.IIdGenerator<long> idGenerator)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, characterId, AppPolicies.CharacterOwnerOrAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            var entriesToRemove = await _context.LootListEntries
                .AsTracking()
                .Where(e => e.LootList == list)
                .ToListAsync();

            var entries = new List<LootListEntry>();

            var brackets = await _context.Brackets
                .AsNoTracking()
                .Where(b => b.Phase == phase)
                .OrderBy(b => b.Index)
                .ToListAsync();

            foreach (var bracket in brackets)
            {
                for (byte rank = bracket.MinRank; rank <= bracket.MaxRank; rank++)
                {
                    int processed = 0;

                    foreach (var entry in entriesToRemove.Where(e => e.Rank == rank))
                    {
                        if (entry.DropId.HasValue)
                        {
                            entries.Add(entry);
                            processed++;
                        }
                        else if (processed < bracket.MaxItems)
                        {
                            entry.ItemId = null;
                            entries.Add(entry);
                            processed++;
                        }
                    }

                    for (; processed < bracket.MaxItems; processed++)
                    {
                        var entry = new LootListEntry(idGenerator.CreateId())
                        {
                            LootList = list,
                            Rank = rank
                        };
                        _context.LootListEntries.Add(entry);
                        entries.Add(entry);
                    }
                }
            }

            entriesToRemove.RemoveAll(entries.Contains);

            _context.LootListEntries.RemoveRange(entriesToRemove);

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListReset", User, props =>
            {
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
                props["Phase"] = list.Phase.ToString();
            });

            var lootLists = await CreateDtosAsync(characterId, null, phase);
            Debug.Assert(lootLists?.Count == 1);
            return lootLists[0];
        }

        [HttpPut("Phase{phase:int}/{characterId:long}")]
        public async Task<ActionResult> PostSpec(long characterId, byte phase, [FromBody] LootListSubmissionDto dto)
        {
            Debug.Assert(dto.MainSpec.HasValue);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, characterId, AppPolicies.CharacterOwnerOrAdmin);
            if (!authorizationResult.Succeeded)
            {
                return Unauthorized();
            }

            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            if (list.Status != LootListStatus.Editing)
            {
                return Problem("Loot List cannot be edited.");
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (!dto.MainSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec.HasValue && !dto.OffSpec.Value.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            list.MainSpec = dto.MainSpec.Value;
            list.OffSpec = dto.OffSpec ?? dto.MainSpec.Value;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/Status")]
        public async Task<ActionResult<byte[]>> PostStatus(long characterId, byte phase, [FromBody] SetLootListStatusDto dto)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            if (list.Status == dto.Status)
            {
                return Problem($"Loot list is already in the {dto.Status} state.");
            }

            if (!TimestampsEqual(list.Timestamp, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            var result = dto.Status switch
            {
                LootListStatus.Editing => await SetStatusToEditingAsync(list),
                LootListStatus.Submitted => await SetStatusToSubmittedAsync(list, dto.SubmitTo),
                LootListStatus.Approved => await SetStatusToApprovedAsync(list),
                LootListStatus.Locked => await SetStatusToLockedAsync(list),
                _ => Problem("Status is not valid."),
            };

            if (result.Result is OkResult)
            {
                _telemetry.TrackEvent("LootListStatusChanged", User, props =>
                {
                    props["CharacterId"] = list.CharacterId.ToString();
                    props["Phase"] = list.Phase.ToString();
                    props["Status"] = list.Status.ToString();
                });
            }

            return result;

            static bool TimestampsEqual(byte[] left, byte[] right)
            {
                if (left.Length != right.Length)
                {
                    return false;
                }

                for (int i = 0; i < left.Length; i++)
                {
                    if (left[i] != right[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        private async Task<ActionResult<byte[]>> SetStatusToEditingAsync(CharacterLootList list)
        {
            AuthorizationResult auth;
            if (list.Status == LootListStatus.Submitted)
            {
                auth = await _authorizationService.AuthorizeAsync(User, list.CharacterId, AppPolicies.CharacterOwnerOrAdmin);

                if (!auth.Succeeded)
                {
                    if (list.SubmittedToId.HasValue)
                    {
                        auth = await _authorizationService.AuthorizeAsync(User, list.SubmittedToId, AppPolicies.RaidLeaderOrAdmin);

                        if (!auth.Succeeded)
                        {
                            return Unauthorized();
                        }
                    }
                    else
                    {
                        return Unauthorized();
                    }
                }
            }
            else if (list.Status == LootListStatus.Approved && list.SubmittedToId.HasValue)
            {
                auth = await _authorizationService.AuthorizeAsync(User, list.SubmittedToId, AppPolicies.RaidLeaderOrAdmin);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }
            else
            {
                auth = await _authorizationService.AuthorizeAsync(User, AppPolicies.Administrator);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }

            list.ApprovedBy = null;
            list.SubmittedToId = null;
            list.Status = LootListStatus.Editing;
            await _context.SaveChangesAsync();
            return list.Timestamp;
        }

        private async Task<ActionResult<byte[]>> SetStatusToSubmittedAsync(CharacterLootList list, long? submitTo)
        {
            if (list.Status == LootListStatus.Editing)
            {
                if (!submitTo.HasValue)
                {
                    ModelState.AddModelError(nameof(SetLootListStatusDto.SubmitTo), "Loot List must be submitted to a raid team.");
                    return ValidationProblem();
                }

                var team = await _context.RaidTeams.FindAsync(submitTo.Value);

                if (team is null)
                {
                    ModelState.AddModelError(nameof(SetLootListStatusDto.SubmitTo), "Raid team does not exist.");
                    return ValidationProblem();
                }

                var auth = await _authorizationService.AuthorizeAsync(User, list.CharacterId, AppPolicies.CharacterOwnerOrAdmin);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }

                list.SubmittedToId = submitTo;
                list.ApprovedBy = null;
                list.Status = LootListStatus.Submitted;
                await _context.SaveChangesAsync();
                return list.Timestamp;
            }
            else
            {
                return Problem("Can't set the status to submitted while the list is not in the editing state.");
            }
        }

        private async Task<ActionResult<byte[]>> SetStatusToApprovedAsync(CharacterLootList list)
        {
            if (list.Status == LootListStatus.Submitted)
            {
                if (!list.SubmittedToId.HasValue)
                {
                    return Problem("Loot List is not assigned to a raid team.");
                }

                var auth = await _authorizationService.AuthorizeAsync(User, list.SubmittedToId.Value, AppPolicies.RaidLeaderOrAdmin);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }
            else if (list.Status == LootListStatus.Locked)
            {
                var auth = await _authorizationService.AuthorizeAsync(User, AppPolicies.Administrator);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Problem($"Can't set the status to approved while the list is in the {list.Status} state.");
            }

            list.ApprovedBy = User.GetDiscordId();
            list.Status = LootListStatus.Approved;
            await _context.SaveChangesAsync();
            return list.Timestamp;
        }

        private async Task<ActionResult<byte[]>> SetStatusToLockedAsync(CharacterLootList list)
        {
            if (list.Status == LootListStatus.Approved)
            {
                if (!list.SubmittedToId.HasValue)
                {
                    return Problem("Loot List is not assigned to a raid team.");
                }

                var auth = await _authorizationService.AuthorizeAsync(User, list.SubmittedToId.Value, AppPolicies.RaidLeaderOrAdmin);

                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
            }
            else
            {
                return Problem($"Can't set the status to locked while the list is in the {list.Status} state.");
            }

            list.Status = LootListStatus.Locked;
            await _context.SaveChangesAsync();
            return list.Timestamp;
        }

        private async Task<IList<LootListDto>?> CreateDtosAsync(long? characterId, long? teamId, byte? phase)
        {
            var lootListQuery = _context.CharacterLootLists.AsNoTracking();
            var passQuery = _context.DropPasses.AsNoTracking().Where(pass => !pass.WonEntryId.HasValue);
            var entryQuery = _context.LootListEntries.AsNoTracking();
            var attendanceQuery = _context.RaidAttendees.AsNoTracking().AsSingleQuery().Where(x => !x.IgnoreAttendance);

            var now = _serverTimeZoneInfo.TimeZoneNow();

            var donationQuery = _context.Donations
                .AsNoTracking()
                .Where(d => d.Month == now.Month && d.Year == now.Year)
                .GroupBy(d => new { d.CharacterId, d.Character.TeamId })
                .Select(g => new { g.Key.CharacterId, g.Key.TeamId, Donated = g.Sum(d => (long)d.CopperAmount) });

            if (characterId.HasValue)
            {
                if (teamId.HasValue)
                {
                    throw new ArgumentException("Either characterId or teamId must be set, but not both.");
                }

                var character = await _context.Characters.AsNoTracking().Where(c => c.Id == characterId).Select(c => new { c.Id, c.TeamId }).FirstOrDefaultAsync();

                if (character is null)
                {
                    return null;
                }

                lootListQuery = lootListQuery.Where(ll => ll.CharacterId == characterId);
                passQuery = passQuery.Where(p => p.CharacterId == characterId);
                entryQuery = entryQuery.Where(e => e.LootList.CharacterId == characterId);
                attendanceQuery = attendanceQuery.Where(a => a.CharacterId == characterId);
                donationQuery = donationQuery.Where(d => d.CharacterId == characterId);
                teamId = character.TeamId;
            }
            else if (teamId.HasValue)
            {
                if (await _context.RaidTeams.CountAsync(team => team.Id == teamId) == 0)
                {
                    return null;
                }

                lootListQuery = lootListQuery.Where(ll => ll.Character.TeamId == teamId || ll.SubmittedToId == teamId);
                entryQuery = entryQuery.Where(e => e.LootList.Character.TeamId == teamId || e.LootList.SubmittedToId == teamId);
                passQuery = passQuery.Where(p => p.Character.TeamId == teamId);
                attendanceQuery = attendanceQuery.Where(a => a.Raid.RaidTeamId == teamId && a.Character.TeamId == teamId);
                donationQuery = donationQuery.Where(d => d.TeamId == teamId);
            }
            else
            {
                throw new ArgumentException("Either characterId or teamId must be set.");
            }

            if (phase.HasValue)
            {
                lootListQuery = lootListQuery.Where(ll => ll.Phase == phase.Value);
                entryQuery = entryQuery.Where(e => e.LootList.Phase == phase.Value);
            }

            var dtos = await lootListQuery
                .Select(ll => new LootListDto
                {
                    ApprovedBy = ll.ApprovedBy,
                    CharacterId = ll.CharacterId,
                    CharacterName = ll.Character.Name,
                    CharacterMemberStatus = ll.Character.MemberStatus,
                    TeamId = ll.Character.TeamId,
                    TeamName = ll.Character.Team!.Name,
                    SubmittedToId = ll.SubmittedToId,
                    SubmittedToName = ll.SubmittedTo!.Name,
                    Status = ll.Status,
                    MainSpec = ll.MainSpec,
                    OffSpec = ll.OffSpec,
                    Phase = ll.Phase,
                    Timestamp = ll.Timestamp
                })
                .ToListAsync();

            var passes = await passQuery
                .Select(pass => new { pass.CharacterId, pass.Drop.ItemId, pass.RelativePriority })
                .ToListAsync();

            var attendances = await attendanceQuery
                .Select(a => new { a.CharacterId, a.Raid.StartedAt.Date })
                .GroupBy(a => a.CharacterId)
                .Select(g => new { CharacterId = g.Key, Count = g.Select(a => a.Date).Distinct().Count() })
                .ToDictionaryAsync(g => g.CharacterId, g => g.Count);

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, characterId, AppPolicies.CharacterOwnerOrAdmin);

            var donations = await donationQuery.ToDictionaryAsync(d => d.CharacterId, d => d.Donated);

            foreach (var dto in dtos)
            {
                attendances.TryGetValue(dto.CharacterId, out int attended);
                donations.TryGetValue(dto.CharacterId, out long donated);
                dto.Bonuses.AddRange(PrioCalculator.GetListBonuses(PrioCalculator.Scope, attended, dto.CharacterMemberStatus, donated));
            }

            var brackets = await _context.Brackets
                .AsNoTracking()
                .OrderBy(b => b.Index)
                .ToListAsync();

            await foreach (var entry in entryQuery
                .OrderByDescending(e => e.Rank)
                .Select(e => new
                {
                    e.Id,
                    e.ItemId,
                    e.Item!.RewardFromId,
                    ItemName = (string?)e.Item!.Name,
                    Won = e.DropId != null,
                    e.Rank,
                    e.LootList.Phase,
                    e.LootList.CharacterId
                })
                .AsAsyncEnumerable())
            {
                var dto = dtos.Find(x => x.CharacterId == entry.CharacterId && x.Phase == entry.Phase);
                if (dto is not null)
                {
                    var bonuses = new List<PriorityBonusDto>();

                    if (entry.ItemId.HasValue && !entry.Won)
                    {
                        var rewardFromId = entry.RewardFromId ?? entry.ItemId.Value;
                        bonuses.AddRange(PrioCalculator.GetItemBonuses(passes.Count(p => p.ItemId == rewardFromId && p.CharacterId == entry.CharacterId)));
                    }

                    var bracket = brackets.Find(b => b.Phase == dto.Phase && entry.Rank <= b.MaxRank && entry.Rank >= b.MinRank);

                    dto.Entries.Add(new LootListEntryDto
                    {
                        Bonuses = bonuses,
                        Id = entry.Id,
                        ItemId = entry.ItemId,
                        RewardFromId = entry.RewardFromId,
                        ItemName = entry.ItemName,
                        Rank = dto.Status == LootListStatus.Locked || authorizationResult.Succeeded ? entry.Rank : 0,
                        Won = entry.Won,
                        Bracket = bracket?.Index ?? 0,
                        BracketAllowsOffspec = bracket?.AllowOffspec ?? false,
                        BracketAllowsTypeDuplicates = bracket?.AllowTypeDuplicates ?? false
                    });
                }
            }

            return dtos;
        }
    }
}
