// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

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

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.Deactivated)
            {
                return Problem("Character has been deactivated.");
            }

            if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
            {
                return Problem("A loot list for that character and phase already exists.");
            }

            if (!dto.MainSpec.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec != default && !dto.OffSpec.IsClass(character.Class))
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
                MainSpec = dto.MainSpec,
                OffSpec = dto.OffSpec,
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

            var scope = await _context.GetCurrentPriorityScopeAsync();

            var observedDates = await _context.Raids
                .AsNoTracking()
                .Where(r => r.RaidTeamId == character.TeamId)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => r.StartedAt.Date)
                .Distinct()
                .Take(scope.ObservedAttendances)
                .ToListAsync();

            var attendance = await _context.RaidAttendees
                .AsNoTracking()
                .Where(x => !x.IgnoreAttendance && x.RemovalId == null && x.CharacterId == character.Id && x.Raid.RaidTeamId == character.TeamId)
                .Select(x => x.Raid.StartedAt.Date)
                .Where(date => observedDates.Contains(date))
                .Distinct()
                .CountAsync();

            var now = _serverTimeZoneInfo.TimeZoneNow();
            var donationMatrix = await _context.GetDonationMatrixAsync(d => d.CharacterId == characterId, scope);
            var donations = donationMatrix.GetCreditForMonth(characterId, now);

            var returnDto = new LootListDto
            {
                ApprovedBy = null,
                Bonuses = PrioCalculator.GetListBonuses(scope, attendance, character.MemberStatus, donations).ToList(),
                CharacterId = character.Id,
                CharacterMemberStatus = character.MemberStatus,
                CharacterName = character.Name,
                MainSpec = list.MainSpec,
                OffSpec = list.OffSpec,
                Phase = list.Phase,
                Status = list.Status,
                TeamId = character.TeamId,
                Timestamp = list.Timestamp
            };

            if (returnDto.TeamId.HasValue)
            {
                returnDto.TeamName = await _context.RaidTeams.AsNoTracking().Where(t => t.Id == returnDto.TeamId).Select(t => t.Name).FirstOrDefaultAsync();
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

            if (list.Status != LootListStatus.Editing)
            {
                return Problem("Loot list is not editable.");
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.Deactivated)
            {
                return Problem("Character has been deactivated.");
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
                            entry.Justification = null;
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
            Debug.Assert(dto.MainSpec != default);

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

            if (character.Deactivated)
            {
                return Problem("Character has been deactivated.");
            }

            if (!dto.MainSpec.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            if (dto.OffSpec != default && !dto.OffSpec.IsClass(character.Class))
            {
                ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
                return ValidationProblem();
            }

            list.MainSpec = dto.MainSpec;
            list.OffSpec = dto.OffSpec;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/SetEditable")]
        public async Task<ActionResult<TimestampDto>> PostSetEditable(long characterId, byte phase, [FromBody] TimestampDto dto)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            AuthorizationResult auth;
            if (list.Status == LootListStatus.Submitted)
            {
                auth = await _authorizationService.AuthorizeAsync(User, list.CharacterId, AppPolicies.CharacterOwnerOrAdmin);
            }
            else
            {
                auth = await _authorizationService.AuthorizeAsync(User, AppPolicies.Administrator);
            }

            if (!auth.Succeeded)
            {
                return Unauthorized();
            }

            if (!ValidateTimestamp(list, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            if (list.Status == LootListStatus.Editing)
            {
                return Problem("Loot list is already editable.");
            }

            var submissions = await _context.LootListTeamSubmissions
                .AsTracking()
                .Where(s => s.LootListCharacterId == list.CharacterId && s.LootListPhase == list.Phase)
                .ToListAsync();

            _context.LootListTeamSubmissions.RemoveRange(submissions);

            list.ApprovedBy = null;
            list.Status = LootListStatus.Editing;
            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListStatusChanged", User, props =>
            {
                props["CharacterId"] = list.CharacterId.ToString();
                props["Phase"] = list.Phase.ToString();
                props["Status"] = list.Status.ToString();
                props["Method"] = "SetEditable";
            });

            return new TimestampDto { Timestamp = list.Timestamp };
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/Submit")]
        public async Task<ActionResult<TimestampDto>> PostSubmit(long characterId, byte phase, [FromBody] SubmitLootListDto dto, [FromServices] DiscordClientProvider dcp)
        {
            if (dto.SubmitTo.Count == 0)
            {
                ModelState.AddModelError(nameof(dto.SubmitTo), "Loot List must be submitted to at least one raid team.");
                return ValidationProblem();
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.Deactivated)
            {
                return Problem("Character has been deactivated.");
            }

            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            var auth = await _authorizationService.AuthorizeAsync(User, list.CharacterId, AppPolicies.CharacterOwnerOrAdmin);

            if (!auth.Succeeded)
            {
                return Unauthorized();
            }

            if (!ValidateTimestamp(list, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            if (list.Status != LootListStatus.Editing && character.TeamId.HasValue)
            {
                return Problem("Can't submit a list that is not editable.");
            }

            var teams = await _context.RaidTeams
                .AsNoTracking()
                .Where(t => dto.SubmitTo.Contains(t.Id))
                .Select(t => new { t.Id, t.Name })
                .ToDictionaryAsync(t => t.Id);

            if (teams.Count != dto.SubmitTo.Count)
            {
                return Problem("One or more raid teams specified do not exist.");
            }

            var submissions = await _context.LootListTeamSubmissions
                .AsTracking()
                .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == list.Phase)
                .ToListAsync();

            foreach (var id in dto.SubmitTo)
            {
                if (submissions.Find(s => s.TeamId == id) is null)
                {
                    _context.LootListTeamSubmissions.Add(new()
                    {
                        LootListCharacterId = list.CharacterId,
                        LootListPhase = list.Phase,
                        TeamId = id
                    });
                }
            }

            foreach (var submission in submissions)
            {
                if (!dto.SubmitTo.Contains(submission.TeamId))
                {
                    _context.LootListTeamSubmissions.Remove(submission);
                }
            }

            list.ApprovedBy = null;

            if (list.Status == LootListStatus.Editing)
            {
                list.Status = LootListStatus.Submitted;
            }

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListStatusChanged", User, props =>
            {
                props["CharacterId"] = list.CharacterId.ToString();
                props["Phase"] = list.Phase.ToString();
                props["Status"] = list.Status.ToString();
                props["Method"] = "Submit";
            });

            const string format = "You have a new application to {0} from {1}. ({2} {3})";

            await foreach (var claim in _context.UserClaims
                .AsNoTracking()
                .Where(claim => claim.ClaimType == AppClaimTypes.RaidLeader)
                .Select(claim => new { claim.UserId, claim.ClaimValue })
                .AsAsyncEnumerable())
            {
                if (long.TryParse(claim.ClaimValue, out var teamId) &&
                    teams.TryGetValue(teamId, out var team) &&
                    submissions.Find(s => s.TeamId == teamId) is null) // Don't notify when submission status doesn't change.
                {
                    await dcp.SendDmAsync(claim.UserId, m => m.WithContent(string.Format(
                        format,
                        team.Name,
                        character.Name,
                        character.Race.GetDisplayName(),
                        list.MainSpec.GetDisplayName(true))));
                }
            }

            return new TimestampDto { Timestamp = list.Timestamp };
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/ApproveOrReject"), Authorize(AppPolicies.RaidLeaderOrAdmin)]
        public async Task<ActionResult<ApproveOrRejectLootListResponseDto>> PostApproveOrReject(long characterId, byte phase, [FromBody] ApproveOrRejectLootListDto dto, [FromServices] DiscordClientProvider dcp)
        {
            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            var team = await _context.RaidTeams.FindAsync(dto.TeamId);

            if (team is null)
            {
                ModelState.AddModelError(nameof(dto.TeamId), "Team does not exist.");
                return ValidationProblem();
            }

            var auth = await _authorizationService.AuthorizeAsync(User, team.Id, AppPolicies.RaidLeaderOrAdmin);

            if (!auth.Succeeded)
            {
                return Unauthorized();
            }

            if (!ValidateTimestamp(list, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            if (list.Status != LootListStatus.Submitted && character.TeamId.HasValue)
            {
                return Problem("Can't approve or reject a list that isn't in the submitted state.");
            }

            var submissions = await _context.LootListTeamSubmissions
                .AsTracking()
                .Where(s => s.LootListCharacterId == list.CharacterId && s.LootListPhase == list.Phase)
                .ToListAsync();

            var teamSubmission = submissions.Find(s => s.TeamId == dto.TeamId);

            if (teamSubmission is null)
            {
                return Unauthorized();
            }

            MemberDto? member = null;

            if (dto.Approved)
            {
                _context.LootListTeamSubmissions.RemoveRange(submissions);

                if (character.TeamId.HasValue)
                {
                    if (character.TeamId != dto.TeamId)
                    {
                        return Problem("That character is not assigned to the specified raid team.");
                    }
                }
                else
                {
                    var idString = character.Id.ToString();
                    var claim = await _context.UserClaims.AsNoTracking()
                        .Where(c => c.ClaimType == AppClaimTypes.Character && c.ClaimValue == idString)
                        .Select(c => new { c.UserId })
                        .FirstOrDefaultAsync();

                    if (claim is not null)
                    {
                        var otherClaims = await _context.UserClaims.AsNoTracking()
                            .Where(c => c.ClaimType == AppClaimTypes.Character && c.UserId == claim.UserId)
                            .Select(c => c.ClaimValue)
                            .ToListAsync();

                        var characterIds = new List<long>();

                        foreach (var otherClaim in otherClaims)
                        {
                            if (long.TryParse(otherClaim, out var cid))
                            {
                                characterIds.Add(cid);
                            }
                        }

                        var existingCharacterName = await _context.Characters
                            .AsNoTracking()
                            .Where(c => characterIds.Contains(c.Id) && c.TeamId == team.Id)
                            .Select(c => c.Name)
                            .FirstOrDefaultAsync();

                        if (existingCharacterName?.Length > 0)
                        {
                            return Problem($"The owner of this character is already on this team as {existingCharacterName}.");
                        }
                    }

                    character.TeamId = dto.TeamId;
                    character.MemberStatus = RaidMemberStatus.FullTrial;
                    character.JoinedTeamAt = _serverTimeZoneInfo.TimeZoneNow();
                }

                list.ApprovedBy = User.GetDiscordId();

                if (list.Status != LootListStatus.Locked)
                {
                    list.Status = LootListStatus.Approved;
                }

                await _context.SaveChangesAsync();

                var characterQuery = _context.Characters.AsNoTracking().Where(c => c.Id == character.Id);
                var scope = await _context.GetCurrentPriorityScopeAsync();

                foreach (var m in await HelperQueries.GetMembersAsync(_context, _serverTimeZoneInfo, characterQuery, scope, team.Id, team.Name, true))
                {
                    member = m;
                    break;
                }
            }
            else
            {
                _context.LootListTeamSubmissions.Remove(teamSubmission);

                if (character.TeamId == team.Id)
                {
                    character.TeamId = null;
                }

                if (submissions.Count == 1)
                {
                    if (list.Status != LootListStatus.Locked)
                    {
                        list.Status = LootListStatus.Editing;
                    }

                    list.ApprovedBy = null;
                }

                await _context.SaveChangesAsync();
            }

            _telemetry.TrackEvent("LootListStatusChanged", User, props =>
            {
                props["CharacterId"] = list.CharacterId.ToString();
                props["Phase"] = list.Phase.ToString();
                props["Status"] = list.Status.ToString();
                props["Method"] = "ApproveOrReject";
            });

            var characterIdString = characterId.ToString();
            var owner = await _context.UserClaims
                .AsNoTracking()
                .Where(c => c.ClaimType == AppClaimTypes.Character && c.ClaimValue == characterIdString)
                .Select(c => c.UserId)
                .FirstOrDefaultAsync();

            if (owner > 0)
            {
                var sb = new StringBuilder("Your application to ")
                    .Append(team.Name)
                    .Append(" for ")
                    .Append(character.Name)
                    .Append(" was ")
                    .Append(dto.Approved ? "approved!" : "rejected.");

                if (dto.Message?.Length > 0)
                {
                    sb.AppendLine()
                        .Append("<@")
                        .Append(User.GetDiscordId())
                        .AppendLine("> said:")
                        .Append("> ")
                        .Append(dto.Message);
                }

                await dcp.SendDmAsync(owner, m => m.WithContent(sb.ToString()));
            }

            return new ApproveOrRejectLootListResponseDto { Timestamp = list.Timestamp, Member = member, LootListStatus = list.Status };
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/Lock"), Authorize(AppPolicies.RaidLeaderOrAdmin)]
        public async Task<ActionResult<TimestampDto>> PostLock(long characterId, byte phase, [FromBody] TimestampDto dto)
        {
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

            var auth = await _authorizationService.AuthorizeAsync(User, character.TeamId, AppPolicies.RaidLeaderOrAdmin);

            if (!auth.Succeeded)
            {
                return Unauthorized();
            }

            if (!ValidateTimestamp(list, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            if (list.Status != LootListStatus.Approved)
            {
                return Problem("Loot list must be approved before locking.");
            }

            list.Status = LootListStatus.Locked;

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("LootListStatusChanged", User, props =>
            {
                props["CharacterId"] = list.CharacterId.ToString();
                props["Phase"] = list.Phase.ToString();
                props["Status"] = list.Status.ToString();
                props["Method"] = "Lock";
            });

            return new TimestampDto { Timestamp = list.Timestamp };
        }

        [HttpPost("Phase{phase:int}/{characterId:long}/Unlock"), Authorize(AppPolicies.Administrator)]
        public async Task<ActionResult<TimestampDto>> PostUnlock(long characterId, byte phase, [FromBody] TimestampDto dto)
        {
            var list = await _context.CharacterLootLists.FindAsync(characterId, phase);

            if (list is null)
            {
                return NotFound();
            }

            if (!ValidateTimestamp(list, dto.Timestamp))
            {
                return Problem("Loot list has been changed. Refresh before trying to update the status again.");
            }

            if (list.Status != LootListStatus.Locked)
            {
                return Problem("Loot list is already unlocked.");
            }

            list.Status = LootListStatus.Approved;

            await _context.SaveChangesAsync();

            var cname = await _context.Characters
                .AsNoTracking()
                .Where(c => c.Id == characterId)
                .Select(c => c.Name)
                .FirstAsync();

            _telemetry.TrackEvent("LootListStatusChanged", User, props =>
            {
                props["CharacterId"] = list.CharacterId.ToString();
                props["CharacterName"] = cname;
                props["Phase"] = list.Phase.ToString();
                props["Status"] = list.Status.ToString();
                props["Method"] = "Unlock";
            });

            //await dcp.SendAsync(635355896020729866, m =>
            //{
            //    var userId = (ulong)User.GetDiscordId().GetValueOrDefault();

            //    var request = Url.ActionContext.HttpContext.Request;

            //    var link = request.Scheme + "://" + request.Host + Url.Content($"~/characters/{cname}/phase/{list.Phase}");
            //    m.WithContent($"<@!{userId}> has just unlocked [{cname}'s Phase {list.Phase} Loot List]({link}).")
            //        .WithAllowedMention(new UserMention(userId));
            //});

            return new TimestampDto { Timestamp = list.Timestamp };
        }

        private static bool ValidateTimestamp(CharacterLootList list, byte[] timestamp)
        {
            if (list.Timestamp.Length != timestamp.Length)
            {
                return false;
            }

            for (int i = 0; i < timestamp.Length; i++)
            {
                if (list.Timestamp[i] != timestamp[i])
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<IList<LootListDto>?> CreateDtosAsync(long? characterId, long? teamId, byte? phase)
        {
            var lootListQuery = _context.CharacterLootLists.AsNoTracking();
            var passQuery = _context.DropPasses.AsNoTracking().Where(pass => !pass.WonEntryId.HasValue && pass.RemovalId == null);
            var entryQuery = _context.LootListEntries.AsNoTracking();
            var attendanceQuery = _context.RaidAttendees.AsNoTracking().AsSingleQuery().Where(x => !x.IgnoreAttendance && x.RemovalId == null);

            var now = _serverTimeZoneInfo.TimeZoneNow();
            var lastMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(-1);

            Expression<Func<Donation, bool>> donationPredicate;

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
                attendanceQuery = attendanceQuery.Where(a => a.CharacterId == characterId && a.Raid.RaidTeamId == character.TeamId);
                donationPredicate = d => d.CharacterId == characterId;
                teamId = character.TeamId;
            }
            else if (teamId.HasValue)
            {
                if (await _context.RaidTeams.CountAsync(team => team.Id == teamId) == 0)
                {
                    return null;
                }

                lootListQuery = lootListQuery.Where(ll => ll.Character.TeamId == teamId || ll.Submissions.Any(s => s.TeamId == teamId));
                entryQuery = entryQuery.Where(e => e.LootList.Character.TeamId == teamId || e.LootList.Submissions.Any(s => s.TeamId == teamId));
                passQuery = passQuery.Where(p => p.Character.TeamId == teamId);
                attendanceQuery = attendanceQuery.Where(a => a.Raid.RaidTeamId == teamId && a.Character.TeamId == teamId);
                donationPredicate = d => d.Character.TeamId == teamId;
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
                    Status = ll.Status,
                    MainSpec = ll.MainSpec,
                    OffSpec = ll.OffSpec,
                    Phase = ll.Phase,
                    Timestamp = ll.Timestamp,
                    SubmittedTo = ll.Submissions.Select(s => s.TeamId).ToList()
                })
                .AsSingleQuery()
                .ToListAsync();

            var passes = await passQuery
                .Select(pass => new { pass.CharacterId, pass.RelativePriority, pass.LootListEntryId })
                .ToListAsync();

            var scope = await _context.GetCurrentPriorityScopeAsync();

            var observedDates = await _context.Raids
                .AsNoTracking()
                .Where(r => r.RaidTeamId == teamId)
                .OrderByDescending(r => r.StartedAt)
                .Select(r => r.StartedAt.Date)
                .Distinct()
                .Take(scope.ObservedAttendances)
                .ToListAsync();

            var attendances = new Dictionary<long, HashSet<DateTime>>();

            await foreach (var record in attendanceQuery.Select(a => new { a.CharacterId, a.Raid.StartedAt.Date }).Where(a => observedDates.Contains(a.Date)).AsAsyncEnumerable())
            {
                if (!attendances.TryGetValue(record.CharacterId, out var dates))
                {
                    attendances[record.CharacterId] = dates = new();
                }

                dates.Add(record.Date);
            }

            var characterAuthorizationLookup = new Dictionary<long, bool>();

            var authorizationResult = await _authorizationService.AuthorizeAsync(User, teamId, AppPolicies.RaidLeaderOrAdmin);

            var donationMatrix = await _context.GetDonationMatrixAsync(donationPredicate, scope);

            foreach (var dto in dtos)
            {
                attendances.TryGetValue(dto.CharacterId, out var attended);
                var characterDonations = donationMatrix.GetCreditForMonth(dto.CharacterId, now);
                dto.Bonuses.AddRange(PrioCalculator.GetListBonuses(scope, attended?.Count ?? 0, dto.CharacterMemberStatus, characterDonations));

                if (authorizationResult.Succeeded)
                {
                    characterAuthorizationLookup[dto.CharacterId] = true;
                }
                else if (!characterAuthorizationLookup.ContainsKey(dto.CharacterId))
                {
                    var ownerAuthResult = await _authorizationService.AuthorizeAsync(User, dto.CharacterId, AppPolicies.CharacterOwner);
                    characterAuthorizationLookup[dto.CharacterId] = ownerAuthResult.Succeeded;
                }
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
                    e.Justification,
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
                        bonuses.AddRange(PrioCalculator.GetItemBonuses(passes.Count(p => p.LootListEntryId == entry.Id)));
                    }

                    var bracket = brackets.Find(b => b.Phase == dto.Phase && entry.Rank <= b.MaxRank && entry.Rank >= b.MinRank);

                    characterAuthorizationLookup.TryGetValue(dto.CharacterId, out var hasRankPrivilege);

                    dto.Entries.Add(new LootListEntryDto
                    {
                        Bonuses = bonuses,
                        Id = entry.Id,
                        ItemId = entry.ItemId,
                        RewardFromId = entry.RewardFromId,
                        ItemName = entry.ItemName,
                        Justification = hasRankPrivilege ? entry.Justification : null,
                        Rank = dto.Status == LootListStatus.Locked || hasRankPrivilege ? entry.Rank : 0,
                        Won = entry.Won,
                        Bracket = hasRankPrivilege && bracket is not null ? bracket.Index : 0,
                        BracketAllowsOffspec = hasRankPrivilege && (bracket?.AllowOffspec ?? false),
                        BracketAllowsTypeDuplicates = hasRankPrivilege && (bracket?.AllowTypeDuplicates ?? false)
                    });
                }
            }

            return dtos;
        }
    }
}
