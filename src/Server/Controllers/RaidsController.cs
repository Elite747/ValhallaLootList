// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
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

        public IAsyncEnumerable<RaidDto> Get([FromServices] TimeZoneInfo realmTimeZoneInfo, int? m = null, int? y = null, long? team = null)
        {
            DateTimeOffset start, end;
            if (m.HasValue || y.HasValue)
            {
                var startUnspecified = new DateTime(y ?? DateTime.Today.Year, m ?? DateTime.Today.Month, 1);
                var startRealm = new DateTimeOffset(startUnspecified, realmTimeZoneInfo.GetUtcOffset(startUnspecified));
                start = startRealm;
                end = startRealm.AddMonths(1);
            }
            else
            {
                end = DateTimeOffset.UtcNow;
                start = end.AddMonths(-1);
            }

            var query = _context.Raids.AsNoTracking().Where(r => r.StartedAt >= start && r.StartedAt < end);

            if (team.HasValue)
            {
                query = query.Where(r => r.RaidTeamId == team.Value);
            }

            return query
                .Select(r => new RaidDto
                {
                    Id = r.Id,
                    Phase = r.Phase,
                    StartedAt = r.StartedAt,
                    TeamId = r.RaidTeamId,
                    TeamName = r.RaidTeam.Name
                })
                .AsAsyncEnumerable();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RaidDto>> Get(long id)
        {
            var dto = await _context.Raids
                .AsNoTracking()
                .Where(raid => raid.Id == id)
                .Select(raid => new RaidDto
                {
                    StartedAt = raid.StartedAt,
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
                    MainSpec = a.Character.CharacterLootLists.FirstOrDefault(ll => ll.Phase == dto.Phase)!.MainSpec,
                    Character = new CharacterDto
                    {
                        Class = a.Character.Class,
                        Editable = isAdmin || a.Character.OwnerId == currentUserId,
                        Gender = a.Character.IsFemale ? Gender.Female : Gender.Male,
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
                .OrderBy(k => k.KilledAt)
                .Select(k => new EncounterKillDto
                {
                    KilledAt = k.KilledAt,
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
                    d.AwardedAt,
                    AwardedBy = (long?)d.AwardedBy,
                    WinnerId = (long?)d.WinnerId,
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
                    AwardedAt = drop.AwardedAt,
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
        public async Task<ActionResult<RaidDto>> Post([FromServices] TimeZoneInfo realmTimeZoneInfo, [FromBody] RaidSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
        {
            var phaseDetails = await _context.PhaseDetails.FindAsync((byte)dto.Phase);

            if (phaseDetails is null)
            {
                ModelState.AddModelError(nameof(dto.Phase), "Phase is not a valid value.");
                return ValidationProblem();
            }

            if (phaseDetails.StartsAt > DateTimeOffset.UtcNow)
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

            if (!await _context.IsLeaderOf(User, team.Id))
            {
                return Unauthorized();
            }

            var raid = new Raid(idGenerator.CreateId())
            {
                Phase = (byte)dto.Phase,
                RaidTeam = team,
                RaidTeamId = team.Id,
                StartedAt = realmTimeZoneInfo.TimeZoneNow()
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
                    MainSpec = a.Character.CharacterLootLists.FirstOrDefault(ll => ll.Phase == dto.Phase)?.MainSpec,
                    Character = new CharacterDto
                    {
                        Id = a.CharacterId,
                        Class = a.Character.Class,
                        Editable = isAdmin || a.Character.OwnerId == currentUserId,
                        Gender = a.Character.IsFemale ? Gender.Female : Gender.Male,
                        Name = a.Character.Name,
                        Race = a.Character.Race,
                        TeamId = a.Character.TeamId,
                        TeamName = a.Character.Team?.Name
                    }
                }).ToList(),
                StartedAt = raid.StartedAt,
                Id = raid.Id,
                Phase = raid.Phase,
                TeamId = team.Id,
                TeamName = team.Name
            });
        }

        [HttpDelete("{id:long}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult> Delete(long id)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
            }

            if (await _context.EncounterKills.CountAsync(ek => ek.RaidId == id) > 0)
            {
                return Problem("Can't delete a raid with recorded kills.");
            }

            _context.Raids.Remove(raid);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id:long}/Attendees"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<AttendanceDto>> PostAttendee(long id, [FromBody] AttendeeSubmissionDto dto)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
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

            var spec = await _context.CharacterLootLists
                .AsNoTracking()
                .Where(ll => ll.Phase == raid.Phase && ll.CharacterId == character.Id)
                .Select(ll => (Specializations?)ll.MainSpec)
                .FirstOrDefaultAsync();

            return new AttendanceDto
            {
                IgnoreAttendance = attendee.IgnoreAttendance,
                IgnoreReason = attendee.IgnoreReason,
                MainSpec = spec,
                Character = new CharacterDto
                {
                    Class = character.Class,
                    Editable = User.IsAdmin() || User.GetDiscordId() == character.OwnerId,
                    Gender = character.IsFemale ? Gender.Female : Gender.Male,
                    Id = character.Id,
                    Name = character.Name,
                    Race = character.Race,
                    TeamId = character.TeamId
                }
            };
        }

        [HttpPut("{id:long}/Attendees/{characterId:long}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<AttendanceDto>> PutAttendee(long id, long characterId, [FromBody] UpdateAttendanceSubmissionDto dto)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
            }

            var attendee = await _context.RaidAttendees.FindAsync(characterId, id);

            if (attendee is null)
            {
                return NotFound();
            }

            if (dto.IgnoreAttendance)
            {
                attendee.IgnoreAttendance = true;
                attendee.IgnoreReason = dto.IgnoreReason;
            }
            else
            {
                attendee.IgnoreAttendance = false;
                attendee.IgnoreReason = null;
            }

            await _context.SaveChangesAsync();

            return new AttendanceDto
            {
                IgnoreAttendance = attendee.IgnoreAttendance,
                IgnoreReason = attendee.IgnoreReason
            };
        }

        [HttpDelete("{id:long}/Attendees/{characterId:long}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult> DeleteAttendee(long id, long characterId)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
            }

            var attendee = await _context.RaidAttendees.FindAsync(characterId, id);

            if (attendee is null)
            {
                return NotFound();
            }

            _context.RaidAttendees.Remove(attendee);

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{id:long}/Kills"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult<EncounterKillDto>> PostKill(long id, [FromBody] KillSubmissionDto dto, [FromServices] TimeZoneInfo realmTimeZoneInfo, [FromServices] IdGen.IIdGenerator<long> idGenerator)
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

            if (await _context.EncounterKills.CountAsync(ek => ek.RaidId == id && ek.EncounterId == dto.EncounterId) > 0)
            {
                return Problem("That boss already has a recorded kill for this raid.");
            }

            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
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
                KilledAt = realmTimeZoneInfo.TimeZoneNow(),
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
                    kill.Drops.Add(new Drop(idGenerator.CreateId())
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
                KilledAt = kill.KilledAt,
                Characters = kill.Characters.Select(c => c.CharacterId).ToList(),
                Drops = kill.Drops.Select(d => new EncounterDropDto
                {
                    Id = d.Id,
                    AwardedAt = d.AwardedAt,
                    AwardedBy = d.AwardedBy,
                    ItemId = d.ItemId,
                    WinnerId = d.Winner?.Id,
                    WinnerName = d.Winner?.Name
                }).ToList(),
                EncounterId = kill.EncounterId,
                EncounterName = encounter.Name
            };
        }

        [HttpDelete("{id:long}/Kills/{encounterId}"), Authorize(AppRoles.LootMaster)]
        public async Task<ActionResult> DeleteKill(long id, string encounterId)
        {
            var raid = await _context.Raids.FindAsync(id);

            if (raid is null)
            {
                return NotFound();
            }

            if (!await _context.IsLeaderOf(User, raid.RaidTeamId))
            {
                return Unauthorized();
            }

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
