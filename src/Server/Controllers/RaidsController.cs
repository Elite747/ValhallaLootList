// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class RaidsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public RaidsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<RaidDto> Get([FromServices] TimeZoneInfo realmTimeZoneInfo, int? m = null, int? y = null, string? team = null)
        {
            DateTime startUtc, endUtc;
            if (m.HasValue || y.HasValue)
            {
                var startUnspecified = new DateTime(y ?? DateTime.Today.Year, m ?? DateTime.Today.Month, 1);
                var startRealm = new DateTimeOffset(startUnspecified, realmTimeZoneInfo.GetUtcOffset(startUnspecified));
                startUtc = startRealm.UtcDateTime;
                endUtc = startRealm.AddMonths(1).UtcDateTime;
            }
            else
            {
                endUtc = DateTime.UtcNow;
                startUtc = endUtc.AddMonths(-1);
            }

            var query = _context.Raids.AsNoTracking().Where(r => r.StartedAtUtc >= startUtc && r.StartedAtUtc < endUtc);

            if (team?.Length > 0)
            {
                query = query.Where(r => r.RaidTeamId == team);
            }

            return query
                .Select(r => new RaidDto
                {
                    Id = r.Id,
                    Phase = r.Phase,
                    StartedAt = new DateTimeOffset(r.StartedAtUtc, TimeSpan.Zero),
                    TeamId = r.RaidTeamId,
                    TeamName = r.RaidTeam.Name
                })
                .AsAsyncEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RaidDto>> Get(string id)
        {
            var dto = await _context.Raids
                .AsNoTracking()
                .Where(raid => raid.Id == id)
                .Select(raid => new RaidDto
                {
                    StartedAt = new DateTimeOffset(raid.StartedAtUtc, TimeSpan.Zero),
                    Id = raid.Id,
                    Phase = raid.Phase,
                    TeamId = raid.RaidTeamId,
                    TeamName = raid.RaidTeam.Name
                })
                .FirstOrDefaultAsync();

            if (dto is null)
            {
                return NotFound();
            }

            var currentUserId = User.GetDiscordId();
            bool isAdmin = User.IsAdmin();

            dto.Attendees = await _context.RaidAttendees
                .AsNoTracking()
                .Where(a => a.RaidId == id)
                .OrderBy(a => a.Character.Name)
                .Select(a => new AttendanceDto
                {
                    IgnoreAttendance = a.IgnoreAttendance,
                    IgnoreReason = a.IgnoreReason,
                    Character = new CharacterDto
                    {
                        Class = a.Character.Class,
                        Editable = isAdmin || a.Character.OwnerId == currentUserId,
                        Gender = a.Character.IsMale ? Gender.Male : Gender.Female,
                        Id = a.CharacterId,
                        Name = a.Character.Name,
                        Race = a.Character.Race,
                        TeamId = a.Character.TeamId,
                        TeamName = a.Character.Team!.Name
                    }
                })
                .ToListAsync();

            dto.Kills = await _context.EncounterKills
                .AsNoTracking()
                .Where(k => k.RaidId == id)
                .OrderBy(k => k.KilledAtUtc)
                .Select(k => new EncounterKillDto
                {
                    KilledAt = new DateTimeOffset(k.KilledAtUtc, TimeSpan.Zero),
                    EncounterId = k.EncounterId,
                    EncounterName = k.Encounter.Name,
                })
                .ToListAsync();

            var killDictionary = dto.Kills.ToDictionary(k => k.EncounterId);

            await foreach (var kek in _context.CharacterEncounterKills
                .AsNoTracking()
                .Where(kek => kek.EncounterKillRaidId == id)
                .Select(kek => new
                {
                    kek.EncounterKillEncounterId,
                    kek.CharacterId
                })
                .AsAsyncEnumerable())
            {
                killDictionary[kek.EncounterKillEncounterId].Characters.Add(kek.CharacterId);
            }

            await foreach (var drop in _context.Drops
                .AsNoTracking()
                .Where(d => d.EncounterKillRaidId == id)
                .Select(d => new
                {
                    d.Id,
                    d.EncounterKillEncounterId,
                    d.AwardedAtUtc,
                    AwardedBy = (string?)d.AwardedBy,
                    WinnerId = (string?)d.WinnerId,
                    WinnerName = (string?)d.Winner!.Name,
                    d.ItemId,
                    ItemName = d.Item.Name
                })
                .OrderBy(d => d.ItemName)
                .AsAsyncEnumerable())
            {
                killDictionary[drop.EncounterKillEncounterId].Drops.Add(new EncounterDropDto
                {
                    Id = drop.Id,
                    AwardedAt = new DateTimeOffset(drop.AwardedAtUtc, TimeSpan.Zero),
                    AwardedBy = drop.AwardedBy,
                    ItemId = drop.ItemId,
                    ItemName = drop.ItemName,
                    WinnerId = drop.WinnerId,
                    WinnerName = drop.WinnerName
                });
            }

            return dto;
        }

        [HttpPost, Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<RaidDto>> Post([FromBody] RaidSubmissionDto dto)
        {
            var phaseDetails = await _context.PhaseDetails.FindAsync((byte)dto.Phase);

            if (phaseDetails is null)
            {
                ModelState.AddModelError(nameof(dto.Phase), "Phase is not a valid value.");
                return ValidationProblem();
            }

            if (phaseDetails.StartsAtUtc > DateTime.UtcNow)
            {
                ModelState.AddModelError(nameof(dto.Phase), "Phase is not yet active.");
                return ValidationProblem();
            }

            var team = await _context.RaidTeams.FindAsync(dto.TeamId);

            if (team is null)
            {
                ModelState.AddModelError(nameof(dto.TeamId), "Raid Team does not exist.");
                return ValidationProblem();
            }

            var raid = new Raid
            {
                Phase = (byte)dto.Phase,
                RaidTeam = team,
                RaidTeamId = team.Id,
                StartedAtUtc = DateTime.UtcNow
            };

            var characters = await _context.Characters.AsTracking().Where(c => dto.Attendees.Contains(c.Id)).ToListAsync();

            for (int i = 0; i < dto.Attendees.Count; i++)
            {
                var charId = dto.Attendees[i];
                var character = characters.Find(c => c.Id == charId);

                if (character is null)
                {
                    ModelState.AddModelError($"{nameof(dto.Attendees)}[{i}]", "Character does not exist.");
                }
                else
                {
                    var attendee = new RaidAttendee
                    {
                        Character = character,
                        CharacterId = character.Id,
                        Raid = raid,
                        RaidId = raid.Id
                    };

                    if (character.TeamId != team.Id)
                    {
                        attendee.IgnoreAttendance = true;
                        attendee.IgnoreReason = "Character was not part of this raid's team at the time of creation.";
                    }

                    raid.Attendees.Add(attendee);
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            _context.Raids.Add(raid);

            await _context.SaveChangesAsync();

            var currentUserId = User.GetDiscordId();
            bool isAdmin = User.IsAdmin();

            return CreatedAtAction(nameof(Get), new { id = raid.Id }, new RaidDto
            {
                Attendees = raid.Attendees.Select(a => new AttendanceDto
                {
                    IgnoreAttendance = a.IgnoreAttendance,
                    IgnoreReason = a.IgnoreReason,
                    Character = new CharacterDto
                    {
                        Id = a.CharacterId,
                        Class = a.Character.Class,
                        Editable = isAdmin || a.Character.OwnerId == currentUserId,
                        Gender = a.Character.IsMale ? Gender.Male : Gender.Female,
                        Name = a.Character.Name,
                        Race = a.Character.Race,
                        TeamId = a.Character.TeamId,
                        TeamName = a.Character.Team!.Name
                    }
                }).ToList(),
                StartedAt = new DateTimeOffset(raid.StartedAtUtc, TimeSpan.Zero),
                Id = raid.Id,
                Phase = raid.Phase,
                TeamId = team.Id,
                TeamName = team.Name
            });
        }

        [HttpPost("{id}/Attendees"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<AttendanceDto>> PostAttendee(string id, [FromBody] AttendeeSubmissionDto dto)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(dto.CharacterId);

            if (character is null)
            {
                ModelState.AddModelError(nameof(dto.CharacterId), "Character does not exist.");
                return ValidationProblem();
            }
            if (await _context.RaidAttendees.AsNoTracking().CountAsync(a => a.CharacterId == character.Id && a.RaidId == raid.Id) != 0)
            {
                ModelState.AddModelError(nameof(dto.CharacterId), "Character is already on this raid's attendance.");
                return ValidationProblem();
            }

            var attendee = new RaidAttendee
            {
                Character = character,
                CharacterId = character.Id,
                IgnoreAttendance = false,
                IgnoreReason = null,
                Raid = raid,
                RaidId = raid.Id
            };

            if (character.TeamId != raid.RaidTeamId)
            {
                attendee.IgnoreAttendance = true;
                attendee.IgnoreReason = "Character was not part of this raid's team at the time of creation.";
            }

            _context.RaidAttendees.Add(attendee);

            await _context.SaveChangesAsync();

            return new AttendanceDto
            {
                IgnoreAttendance = attendee.IgnoreAttendance,
                IgnoreReason = attendee.IgnoreReason,
                Character = new CharacterDto
                {
                    Class = character.Class,
                    Editable = User.IsAdmin() || User.GetDiscordId() == character.OwnerId,
                    Gender = character.IsMale ? Gender.Male : Gender.Female,
                    Id = character.Id,
                    Name = character.Name,
                    Race = character.Race,
                    TeamId = character.TeamId
                }
            };
        }

        [HttpDelete("{id}/Attendees/{characterId}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult> DeleteAttendee(string id, string characterId)
        {
            var attendee = await _context.RaidAttendees.FindAsync(characterId, id);

            if (attendee is null)
            {
                return NotFound();
            }

            _context.RaidAttendees.Remove(attendee);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id}/Kills"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<EncounterKillDto>> PostKill(string id, [FromBody] KillSubmissionDto dto)
        {
            if (dto.Drops.Count == 0)
            {
                ModelState.AddModelError(nameof(dto.Drops), "At least one item drop must be specified.");
                return ValidationProblem();
            }

            if (dto.Characters.Count == 0)
            {
                ModelState.AddModelError(nameof(dto.Characters), "At least one character must be specified.");
                return ValidationProblem();
            }

            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            var encounter = await _context.Encounters
                .AsTracking()
                .Where(e => e.Id == dto.EncounterId)
                .Select(e => new { e.Id, e.Name, e.Instance.Phase })
                .FirstOrDefaultAsync();

            if (encounter is null)
            {
                ModelState.AddModelError(nameof(dto.EncounterId), "Encounter does not exist.");
                return ValidationProblem();
            }

            if (encounter.Phase != raid.Phase)
            {
                ModelState.AddModelError(nameof(dto.EncounterId), "Encounter is not part of the same phase as the raid.");
                return ValidationProblem();
            }

            var kill = new EncounterKill
            {
                KilledAtUtc = DateTime.UtcNow,
                EncounterId = encounter.Id,
                Raid = raid,
                RaidId = raid.Id
            };

            var characters = await _context.Characters.AsTracking().Where(c => dto.Characters.Contains(c.Id)).ToListAsync();

            for (int i = 0; i < dto.Characters.Count; i++)
            {
                var charId = dto.Characters[i];
                var character = characters.Find(c => c.Id == charId);

                if (character is null)
                {
                    ModelState.AddModelError($"{nameof(dto.Characters)}[{i}]", "Character does not exist.");
                }
                else
                {
                    kill.Characters.Add(new CharacterEncounterKill
                    {
                        Character = character,
                        CharacterId = character.Id,
                        EncounterKill = kill,
                        EncounterKillEncounterId = kill.EncounterId,
                        EncounterKillRaidId = kill.RaidId
                    });
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            var items = await _context.Items.AsTracking().Where(i => dto.Drops.Contains(i.Id)).ToListAsync();

            for (int i = 0; i < dto.Drops.Count; i++)
            {
                var itemId = dto.Drops[i];
                var item = items.Find(c => c.Id == itemId);

                if (item is null)
                {
                    ModelState.AddModelError($"{nameof(dto.Drops)}[{i}]", "Item does not exist.");
                }
                else if (item.EncounterId != encounter.Id)
                {
                    ModelState.AddModelError($"{nameof(dto.Drops)}[{i}]", "Item does not belong to the specified encounter.");
                }
                else
                {
                    kill.Drops.Add(new Drop
                    {
                        EncounterKill = kill,
                        EncounterKillEncounterId = kill.EncounterId,
                        EncounterKillRaidId = kill.RaidId,
                        Item = item,
                        ItemId = item.Id
                    });
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            _context.EncounterKills.Add(kill);

            await _context.SaveChangesAsync();

            return new EncounterKillDto
            {
                KilledAt = new DateTimeOffset(kill.KilledAtUtc, TimeSpan.Zero),
                Characters = kill.Characters.Select(c => c.CharacterId).ToList(),
                Drops = kill.Drops.Select(d => new EncounterDropDto
                {
                    Id = d.Id,
                    AwardedAt = new DateTimeOffset(d.AwardedAtUtc, TimeSpan.Zero),
                    AwardedBy = d.AwardedBy,
                    ItemId = d.ItemId,
                    WinnerId = d.Winner?.Id,
                    WinnerName = d.Winner?.Name
                }).ToList(),
                EncounterId = kill.EncounterId,
                EncounterName = encounter.Name
            };
        }

        [HttpDelete("{id}/Kills/{encounterId}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult> DeleteKill(string id, string encounterId)
        {
            var kill = await _context.EncounterKills.FindAsync(encounterId, id);

            if (kill is null)
            {
                return NotFound();
            }

            var drops = await _context.Drops.AsTracking().Where(drop => drop.EncounterKillEncounterId == encounterId && drop.EncounterKillRaidId == id).ToListAsync();

            if (drops.Any(d => d.WinnerId is not null))
            {
                return Problem("Can't delete a kill that has items already awarded.", statusCode: 400);
            }

            _context.Drops.RemoveRange(drops);
            _context.EncounterKills.Remove(kill);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
