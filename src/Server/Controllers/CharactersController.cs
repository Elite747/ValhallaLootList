// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;

        public CharactersController(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
        }

        [HttpGet]
        public IAsyncEnumerable<CharacterDto> Get(bool owned = false, string? team = null)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            const bool isAdmin = false; // TODO: role checking which should override editability.

            var query = _context.Characters.AsNoTracking();

            if (owned)
            {
                query = query.Where(c => c.OwnerId == currentUserId);
            }

            if (team?.Length > 0)
            {
                if (string.Equals(team, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    query = query.Where(c => c.TeamId == null);
                }
                else
                {
                    query = query.Where(c => c.TeamId == team || c.Team!.Name == team);
                }
            }

            return query
                .OrderBy(c => c.Name)
                .Select(c => new CharacterDto
                {
                    Class = c.Class,
                    Id = c.Id,
                    Name = c.Name,
                    Race = c.Race,
                    TeamId = c.TeamId,
                    TeamName = c.Team!.Name,
                    Gender = c.IsMale ? Gender.Male : Gender.Female,
                    Editable = isAdmin || c.OwnerId == currentUserId
                })
                .AsAsyncEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacterDto>> Get(string id)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            const bool isAdmin = false; // TODO: role checking which should override editability.

            var character = await _context.Characters
                .AsNoTracking()
                .Where(c => c.Id == id || c.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CharacterDto
                {
                    Class = c.Class,
                    Id = c.Id,
                    Name = c.Name,
                    Race = c.Race,
                    TeamId = c.Team!.Id,
                    TeamName = c.Team.Name,
                    Gender = c.IsMale ? Gender.Male : Gender.Female,
                    Editable = isAdmin || c.OwnerId == currentUserId
                })
                .FirstOrDefaultAsync();

            if (character is null)
            {
                return NotFound();
            }

            return character;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<CharacterDto>> Post([FromBody] CharacterSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            Debug.Assert(dto.Name?.Length > 1);
            Debug.Assert(dto.Class.HasValue);
            Debug.Assert(dto.Race.HasValue);

            var normalizedName = NormalizeName(dto.Name);

            if (await _context.Characters.AsNoTracking().AnyAsync(c => c.Name.Equals(normalizedName)))
            {
                ModelState.AddModelError(nameof(dto.Name), "A character with that name already exists.");
                return ValidationProblem();
            }

            if (!dto.Race.Value.IsValidRace())
            {
                ModelState.AddModelError(nameof(dto.Race), "Race selection is not valid.");
            }

            if (!dto.Class.Value.IsSingleClass())
            {
                ModelState.AddModelError(nameof(dto.Class), "Class selection is not valid.");
                return ValidationProblem();
            }

            if ((dto.Class.Value & dto.Race.Value.GetClasses()) == 0)
            {
                ModelState.AddModelError(nameof(dto.Class), "Class is not available to the selected race.");
                return ValidationProblem();
            }

            var character = new Character
            {
                Class = dto.Class.Value,
                MemberStatus = RaidMemberStatus.FullTrial,
                IsLeader = false,
                Name = normalizedName,
                Race = dto.Race.Value,
                IsMale = dto.Gender == Gender.Male,
                OwnerId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            _context.Characters.Add(character);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = character.Id }, new CharacterDto
            {
                Class = character.Class,
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Gender = character.IsMale ? Gender.Male : Gender.Female
            });
        }

        [HttpGet("{id}/LootLists")]
        public async Task<ActionResult<IAsyncEnumerable<LootListDto>>> GetLootLists(string id)
        {
            var character = await FindCharacterByIdOrNameAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            var dtos = await CreateDtosAsync(character.Id, character.TeamId, null);

            return Ok(dtos.Values);
        }

        [HttpGet("{id}/LootLists/{phase:int}")]
        public async Task<ActionResult<LootListDto>> GetLootList(string id, byte phase)
        {
            var character = await FindCharacterByIdOrNameAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            var dtos = await CreateDtosAsync(character.Id, character.TeamId, phase);

            if (dtos.TryGetValue(phase, out var dto))
            {
                return dto;
            }

            return NotFound();
        }

        [HttpPost("{id}/LootLists/{phase:int}"), Authorize]
        public async Task<ActionResult<LootListDto>> PostLootList(string id, byte phase, [FromBody] LootListSubmissionDto dto)
        {
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
            Debug.Assert(dto.Items is not null);

            var character = await FindCharacterByIdOrNameAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            const bool isAdmin = false; // TODO: role checking which should override editability.

            if (!isAdmin && character.OwnerId != currentUserId)
            {
                return Unauthorized();
            }

            if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
            {
                return Problem(statusCode: 400, title: "Bad Request", detail: "A loot list for that character and phase already exists.");
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
                Locked = false,
                MainSpec = dto.MainSpec.Value,
                OffSpec = dto.OffSpec ?? dto.MainSpec.Value,
                Phase = phase
            };

            var allItemIds = new HashSet<uint>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                var bracketTemplate = bracketTemplates.Find(bt => rank >= bt.MinRank && rank <= bt.MaxRank);

                if (bracketTemplate is null)
                {
                    ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} is not allowed.");
                }
                else if (itemIds?.Length > 0)
                {
                    if (itemIds.Length > bracketTemplate.MaxItems)
                    {
                        ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} can only have up to {bracketTemplate.MaxItems} items.");
                    }
                    else
                    {
                        for (int col = 0; col < itemIds.Length; col++)
                        {
                            uint itemId = itemIds[col];
                            if (itemId > 0 && !allItemIds.Add(itemId))
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Duplicate items are not allowed.");
                            }
                        }
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            var items = await _context.Items
                .AsNoTracking()
                .Where(item => allItemIds.Contains(item.Id))
                .Select(item => new { item.Id, item.Name, item.Slot, item.Type, Phase = (item.RewardFromId != null ? item.RewardFrom!.Encounter!.Instance.Phase : item.Encounter!.Instance!.Phase) }) // TODO: simplify phase query
                .ToDictionaryAsync(item => item.Id);

            var bothSpecs = dto.OffSpec.HasValue ? (dto.MainSpec.Value | dto.OffSpec.Value) : dto.MainSpec.Value;

            var restrictions = (await _context.ItemRestrictions
                .AsNoTracking()
                .Where(r => allItemIds.Contains(r.ItemId) && r.RestrictionLevel == ItemRestrictionLevel.Unequippable && (r.Specializations & bothSpecs) != 0)
                .ToListAsync())
                .ToLookup(r => r.ItemId);

            var bracketItemGroups = new HashSet<ItemGroup>();

            foreach (var (rank, itemIds) in dto.Items)
            {
                if (itemIds?.Length > 0)
                {
                    var bracketTemplate = bracketTemplates.First(bt => rank >= bt.MinRank && rank <= bt.MaxRank);
                    var bracketSpec = bracketTemplate.AllowOffspec ? bothSpecs : dto.MainSpec.Value;

                    for (int col = 0; col < itemIds.Length; col++)
                    {
                        uint itemId = itemIds[col];
                        if (itemId > 0)
                        {
                            if (items.TryGetValue(itemId, out var item))
                            {
                                if (phase != item.Phase)
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", "Item is not in the same content phase as the specified loot list phase.");
                                }

                                if (!bracketTemplate.AllowTypeDuplicates && !bracketItemGroups.Add(new ItemGroup(item.Type, item.Slot)))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", $"Cannot have multiple items of the same type in Bracket {bracketTemplates.IndexOf(bracketTemplate) + 1}.");
                                }

                                foreach (var restriction in restrictions[itemId].Where(r => (r.Specializations & bracketSpec) != 0))
                                {
                                    ModelState.AddModelError($"Items[{rank}][{col}]", restriction.Reason);
                                }

                                list.Entries.Add(new LootListEntry
                                {
                                    ItemId = itemId,
                                    LootList = list,
                                    PassCount = 0,
                                    Rank = (byte)rank,
                                    Won = false
                                });
                            }
                            else
                            {
                                ModelState.AddModelError($"Items[{rank}][{col}]", "Item does not exist.");
                            }
                        }
                    }
                }

                bracketItemGroups.Clear();
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            _context.Add(list);

            await _context.SaveChangesAsync();

            var dtos = await CreateDtosAsync(character.Id, character.TeamId, phase);

            return CreatedAtAction(nameof(GetLootList), new { id, phase }, dtos[phase]);
        }

        private async Task<Character?> FindCharacterByIdOrNameAsync(string idOrName, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.AsTracking().FirstOrDefaultAsync<Character>(c => c.Id == idOrName || c.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase), cancellationToken);
        }

        private static string NormalizeName(string name)
        {
            return string.Create(name.Length, name, (span, name) =>
            {
                span[0] = char.ToUpperInvariant(name[0]);

                for (int i = 1; i < span.Length; i++)
                {
                    span[i] = char.ToLowerInvariant(name[i]);
                }
            });
        }

        private async Task<Dictionary<byte, LootListDto>> CreateDtosAsync(string characterId, string? teamId, byte? phase)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            const bool isAdmin = false; // TODO: role checking which should override editability.

            var lootListQuery = _context.CharacterLootLists.AsNoTracking().Where(ll => ll.CharacterId == characterId);

            if (phase.HasValue)
            {
                lootListQuery = lootListQuery.Where(ll => ll.Phase == phase.Value);
            }

            var dtos = await lootListQuery
                .Select(ll => new LootListDto
                {
                    ApprovedBy = ll.ApprovedBy,
                    CharacterId = ll.CharacterId,
                    CharacterName = ll.Character.Name,
                    CharacterMemberStatus = ll.Character.MemberStatus,
                    Owned = isAdmin || ll.Character.OwnerId == currentUserId,
                    TeamId = ll.Character.TeamId,
                    TeamName = ll.Character.Team!.Name,
                    Locked = ll.Locked,
                    MainSpec = ll.MainSpec,
                    OffSpec = ll.OffSpec,
                    Phase = ll.Phase
                })
                .ToDictionaryAsync(ll => ll.Phase);

            // TODO: role check to make sure user is allowed to view prios & ranks while the loot list is unlocked.

            var passRecords = await _context.DropPasses
                .AsNoTracking()
                .Where(dp => dp.CharacterId == characterId)
                .Select(dp => new { dp.DropItemId, dp.RelativePriority })
                .ToListAsync();

            var winRecords = new Dictionary<uint, int>();

            await foreach (var drop in _context.Drops
                .AsNoTracking()
                .Where(d => d.WinnerId == characterId)
                .Select(d => d.ItemId)
                .AsAsyncEnumerable())
            {
                winRecords.TryGetValue(drop, out int i);
                winRecords[drop] = i + 1;
            }

            var offset = TimeSpanHelpers.GetTimeZoneOffsetString(_serverTimeZoneInfo.BaseUtcOffset);
            var attendances = teamId?.Length > 0 ? await _context.RaidAttendees
                .AsNoTracking()
                .AsSingleQuery()
                .Where(x => !x.IgnoreAttendance && x.CharacterId == characterId && x.Raid.RaidTeamId == teamId)
                .Select(x => MySqlTranslations.ConvertTz(x.Raid.StartedAtUtc, "+00:00", offset).Date)
                .Distinct()
                .OrderByDescending(x => x)
                .Take(PrioCalculator.ObservedRaidsForAttendance)
                .CountAsync() : 0;

            var entryQuery = _context.LootListEntries.AsNoTracking().Where(e => e.LootList.CharacterId == characterId);

            if (phase.HasValue)
            {
                entryQuery = entryQuery.Where(e => e.LootList.Phase == phase.Value);
            }

            await foreach (var entry in entryQuery
                .OrderByDescending(e => e.Rank)
                .Select(e => new
                {
                    e.Id,
                    e.ItemId,
                    e.Item!.RewardFromId,
                    ItemName = (string?)e.Item!.Name,
                    e.Rank,
                    e.Won,
                    e.LootList.Phase
                })
                .AsAsyncEnumerable())
            {
                if (dtos.TryGetValue(entry.Phase, out var dto))
                {
                    bool won = false;
                    int? prio = null;

                    if (entry.ItemId.HasValue)
                    {
                        var rewardFromId = entry.RewardFromId ?? entry.ItemId.Value;

                        if (!won && winRecords.TryGetValue(rewardFromId, out int winCount) && winCount > 0)
                        {
                            won = true;
                            winRecords[rewardFromId] = winCount - 1;
                        }

                        if (!won && dto.Locked)
                        {
                            int loss = 0, underPrio = 0;

                            foreach (var passRecord in passRecords.Where(x => x.DropItemId == rewardFromId))
                            {
                                if (passRecord.RelativePriority >= 0)
                                {
                                    loss++;
                                }
                                else
                                {
                                    underPrio++;
                                }
                            }

                            prio = PrioCalculator.CalculatePrio(entry.Rank, attendances, dto.CharacterMemberStatus, loss, underPrio);
                        }
                    }

                    bool showRanks = dto.Locked || dto.Owned;

                    dto.Entries.Add(new LootListEntryDto
                    {
                        Id = entry.Id,
                        ItemId = entry.ItemId,
                        ItemName = entry.ItemName,
                        Prio = showRanks ? prio : null,
                        Rank = showRanks ? entry.Rank : 0,
                        Won = won || entry.Won
                    });
                }
            }

            return dtos;
        }
    }
}
