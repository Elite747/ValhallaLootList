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
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CharactersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CharactersController(ApplicationDbContext context)
        {
            _context = context;
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
                    query = query.Where(c => c.TeamId == team);
                }
            }

            return query
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

            if (await _context.Characters.AnyAsync(c => c.Name.Equals(normalizedName)))
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

            return Ok(EnumerateLootLists(character));

            async IAsyncEnumerable<LootListDto> EnumerateLootLists(Character character)
            {
                var allEntries = await _context.LootListEntries
                    .AsNoTracking()
                    .Where(e => e.LootList.CharacterId == character.Id)
                    .Select(e => new
                    {
                        e.Id,
                        e.ItemId,
                        ItemName = (string?)e.Item!.Name,
                        e.LootList.Phase,
                        e.PassCount,
                        e.Rank,
                        e.Won
                    })
                    .ToListAsync();

                await foreach (var list in _context.CharacterLootLists
                    .AsNoTracking()
                    .Where(ll => ll.CharacterId == character.Id)
                    .OrderBy(ll => ll.Phase)
                    .AsAsyncEnumerable())
                {
                    var dto = new LootListDto
                    {
                        ApprovedBy = list.ApprovedBy,
                        CharacterId = character.Id,
                        CharacterName = character.Name,
                        MainSpec = list.MainSpec,
                        OffSpec = list.OffSpec,
                        Phase = list.Phase,
                        Locked = list.Locked
                    };

                    foreach (var entry in allEntries.Where(e => e.Phase == list.Phase).OrderByDescending(e => e.Rank))
                    {
                        dto.Entries.Add(new LootListEntryDto
                        {
                            Id = entry.Id,
                            ItemId = entry.ItemId,
                            ItemName = entry.ItemName,
                            PassCount = entry.PassCount,
                            Rank = entry.Rank,
                            Won = entry.Won
                        });
                    }

                    yield return dto;
                }
            }
        }

        [HttpGet("{id}/LootLists/{phase:int}")]
        public async Task<ActionResult<LootListDto>> GetLootList(string id, byte phase)
        {
            var dto = await _context.CharacterLootLists
                .AsNoTracking()
                .Where(ll => ll.Phase == phase && (ll.CharacterId == id || ll.Character.Name.Equals(id, StringComparison.OrdinalIgnoreCase)))
                .Select(ll => new LootListDto
                {
                    ApprovedBy = ll.ApprovedBy,
                    CharacterId = ll.CharacterId,
                    CharacterName = ll.Character.Name,
                    Locked = ll.Locked,
                    MainSpec = ll.MainSpec,
                    OffSpec = ll.OffSpec,
                    Phase = ll.Phase
                })
                .FirstOrDefaultAsync();

            if (dto is null)
            {
                return NotFound();
            }

            dto.Phase = phase;

            dto.Entries = await _context.LootListEntries
                .AsNoTracking()
                .Where(e => e.LootList.CharacterId == dto.CharacterId && e.LootList.Phase == phase)
                .OrderByDescending(e => e.Rank)
                .Select(e => new LootListEntryDto
                {
                    Id = e.Id,
                    ItemId = e.ItemId,
                    ItemName = e.Item!.Name,
                    PassCount = e.PassCount,
                    Rank = e.Rank,
                    Won = e.Won
                })
                .ToListAsync();

            return dto;
        }

        [HttpGet("LootListTemplate")]
        public ActionResult<IEnumerable<BracketTemplate>> GetLootListTemplate(byte phase = 1)
        {
            var template = GetBracketTemplates(phase);

            if (template is null)
            {
                return NotFound();
            }

            return template;
        }

        [HttpPost("{id}/LootLists/{phase:int}"), Authorize]
        public async Task<ActionResult<LootListDto>> PostLootList(string id, byte phase, [FromBody] LootListSubmissionDto dto)
        {
            var template = GetBracketTemplates(phase);

            if (template is null)
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

            if (await _context.CharacterLootLists.AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
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
                var bracketTemplate = Array.Find(template, bt => rank >= bt.LowestRank && rank <= bt.HighestRank);

                if (bracketTemplate is null)
                {
                    ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} is not allowed.");
                }
                else if (itemIds?.Length > 0)
                {
                    if (itemIds.Length > bracketTemplate.ItemsPerRow)
                    {
                        ModelState.AddModelError($"Items[{rank}]", $"Rank {rank} can only have up to {bracketTemplate.ItemsPerRow} items.");
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
                .Select(item => new { item.Id, item.Name, item.Slot, item.Type, item.Encounter.Instance.Phase })
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
                    var bracketTemplate = template.First(bt => rank >= bt.LowestRank && rank <= bt.HighestRank);
                    var bracketSpec = bracketTemplate.AllowOffSpec ? bothSpecs : dto.MainSpec.Value;

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
                                    ModelState.AddModelError($"Items[{rank}][{col}]", $"Cannot have multiple items of the same type in Bracket {Array.IndexOf(template, bracketTemplate) + 1}.");
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

            var returnDto = new LootListDto
            {
                ApprovedBy = list.ApprovedBy,
                CharacterId = list.CharacterId,
                CharacterName = character.Name,
                Phase = list.Phase,
                Locked = list.Locked,
                Entries = new()
            };

            foreach (var entry in list.Entries)
            {
                var entryDto = new LootListEntryDto
                {
                    Id = entry.Id,
                    ItemId = entry.ItemId,
                    PassCount = entry.PassCount,
                    Rank = entry.Rank,
                    Won = entry.Won
                };

                if (entry.ItemId.HasValue && items.TryGetValue(entry.ItemId.Value, out var item))
                {
                    entryDto.ItemName = item.Name;
                }

                returnDto.Entries.Add(entryDto);
            }

            return CreatedAtAction(nameof(GetLootList), new { id, phase }, returnDto);
        }

        private async Task<Character?> FindCharacterByIdOrNameAsync(string idOrName, CancellationToken cancellationToken = default)
        {
            return await _context.Characters.FirstOrDefaultAsync<Character>(c => c.Id == idOrName || c.Name.Equals(idOrName, StringComparison.OrdinalIgnoreCase), cancellationToken);
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

        private static BracketTemplate[]? GetBracketTemplates(byte phase)
        {
            if (phase == 1)
            {
                return new[] {
                    new BracketTemplate
                    {
                        HighestRank = 18,
                        LowestRank = 15,
                        ItemsPerRow = 1,
                        AllowOffSpec = false,
                        AllowTypeDuplicates = false
                    },
                    new BracketTemplate
                    {
                        HighestRank = 14,
                        LowestRank = 11,
                        ItemsPerRow = 1,
                        AllowOffSpec = false,
                        AllowTypeDuplicates = false
                    },
                    new BracketTemplate
                    {
                        HighestRank = 10,
                        LowestRank = 7,
                        ItemsPerRow = 2,
                        AllowOffSpec = false,
                        AllowTypeDuplicates = false
                    },
                    new BracketTemplate
                    {
                        HighestRank = 6,
                        LowestRank = 1,
                        ItemsPerRow = 2,
                        AllowOffSpec = true,
                        AllowTypeDuplicates = true
                    },
                };
            }

            return null;
        }
    }
}
