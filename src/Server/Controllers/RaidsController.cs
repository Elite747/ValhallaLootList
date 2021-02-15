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
    [ApiController]
    [Route("api/v1/[controller]")]
    public class RaidsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RaidsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<RaidDto> Get(string team, int m, int y, [FromServices] TimeZoneInfo realmTimeZoneInfo)
        {
            var startUnspecified = new DateTime(y, m, 1);
            var startRealm = new DateTimeOffset(startUnspecified, realmTimeZoneInfo.GetUtcOffset(startUnspecified));
            var startUtc = startRealm.UtcDateTime;
            var endUtc = startRealm.AddMonths(1).UtcDateTime;

            return _context.Raids
                .AsNoTracking()
                .Where(r => r.RaidTeamId == team && r.StartedAtUtc >= startUtc && r.StartedAtUtc < endUtc)
                .Select(r => new RaidDto
                {
                    Id = r.Id,
                    StartedAt = new DateTimeOffset(r.StartedAtUtc, TimeSpan.Zero),
                    InstanceId = r.InstanceId,
                    InstanceName = r.Instance.Name
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
                    Attendees = raid.Attendees.Select(a => new RaidAttendeeDto
                    {
                        IgnoreAttendance = a.IgnoreAttendance,
                        CharacterId = a.CharacterId,
                        CharacterName = a.Character.Name,
                        IgnoreReason = a.IgnoreReason,
                        UsingOffspec = a.UsingOffspec
                    }).ToList(),
                    Kills = raid.Kills.Select(k => new EncounterKillDto
                    {
                        KilledAt = new DateTimeOffset(k.KilledAtUtc, TimeSpan.Zero),
                        EncounterId = k.EncounterId,
                        EncounterName = k.Encounter.Name,
                        Characters = k.Characters.Select(c => c.CharacterId).ToList(),
                        Drops = k.Drops.Select(d => new EncounterDropDto
                        {
                            AwardedAt = new DateTimeOffset(d.AwardedAtUtc, TimeSpan.Zero),
                            AwardedBy = d.AwardedBy,
                            WinnerId = d.Winner!.Id,
                            WinnerName = d.Winner.Name,
                            ItemId = d.ItemId
                        }).ToList()
                    }).ToList(),
                    StartedAt = new DateTimeOffset(raid.StartedAtUtc, TimeSpan.Zero),
                    Id = raid.Id,
                    InstanceId = raid.InstanceId,
                    InstanceName = raid.Instance.Name
                })
                .FirstOrDefaultAsync();

            if (dto is null)
            {
                return NotFound();
            }

            return dto;
        }

        [HttpPost, Authorize]
        public async Task<ActionResult<RaidDto>> Post([FromBody] RaidSubmissionDto dto)
        {
            var instance = await _context.Instances.FindAsync(dto.InstanceId);

            if (instance is null)
            {
                ModelState.AddModelError(nameof(dto.InstanceId), "Instance does not exist.");
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
                Instance = instance,
                InstanceId = instance.Id,
                RaidTeam = team,
                RaidTeamId = team.Id,
                StartedAtUtc = DateTime.UtcNow
            };

            var charIds = dto.Attendees.Select(a => a.CharacterId).ToHashSet();
            var characters = await _context.Characters.Where(c => charIds.Contains(c.Id)).ToListAsync();

            for (int i = 0; i < dto.Attendees.Count; i++)
            {
                var attendee = dto.Attendees[i];
                var character = characters.Find(c => c.Id == attendee.CharacterId);

                if (character is null)
                {
                    ModelState.AddModelError($"{nameof(dto.Attendees)}[{i}].{nameof(attendee.CharacterId)}", "Character does not exist.");
                }
                else
                {
                    raid.Attendees.Add(new RaidAttendee
                    {
                        IgnoreAttendance = attendee.IgnoreAttendance,
                        Character = character,
                        CharacterId = character.Id,
                        IgnoreReason = attendee.IgnoreReason,
                        Raid = raid,
                        RaidId = raid.Id,
                        UsingOffspec = attendee.UsingOffspec
                    });
                }
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            _context.Raids.Add(raid);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = raid.Id }, new RaidDto
            {
                Attendees = raid.Attendees.Select(a => new RaidAttendeeDto
                {
                    CharacterId = a.CharacterId,
                    IgnoreAttendance = a.IgnoreAttendance,
                    IgnoreReason = a.IgnoreReason,
                    UsingOffspec = a.UsingOffspec
                }).ToList(),
                StartedAt = new DateTimeOffset(raid.StartedAtUtc, TimeSpan.Zero),
                Id = raid.Id,
                InstanceId = raid.InstanceId,
                InstanceName = instance.Name
            });
        }

        [HttpPost("{id}/Kills"), Authorize]
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

            var encounter = await _context.Encounters.FindAsync(dto.EncounterId);

            if (encounter is null)
            {
                ModelState.AddModelError(nameof(dto.EncounterId), "Encounter does not exist.");
                return ValidationProblem();
            }

            if (encounter.InstanceId != raid.InstanceId)
            {
                ModelState.AddModelError(nameof(dto.EncounterId), "Encounter is not part of the same instance as the raid.");
                return ValidationProblem();
            }

            var kill = new EncounterKill
            {
                KilledAtUtc = DateTime.UtcNow,
                Encounter = encounter,
                EncounterId = encounter.Id,
                Raid = raid,
                RaidId = raid.Id
            };

            var characters = await _context.Characters.Where(c => dto.Characters.Contains(c.Id)).ToListAsync();

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

            var items = await _context.Items.Where(i => dto.Drops.Contains(i.Id)).ToListAsync();

            for (int i = 0; i < dto.Drops.Count; i++)
            {
                var itemId = dto.Drops[i];
                var item = items.Find(c => c.Id == itemId);

                if (item is null)
                {
                    ModelState.AddModelError($"{nameof(dto.Drops)}[{i}]", "Item does not exist.");
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

        [HttpPut("{id}/Kills/{encounterId}/Drops/{itemId:int}"), Authorize]
        public async Task<ActionResult<EncounterDropDto>> PutDrop(string id, string encounterId, uint itemId, [FromBody] AwardDropSubmissionDto dto, [FromServices] PrioCalculator prioCalculator)
        {
            var now = DateTime.UtcNow;
            var drop = await _context.Drops.FindAsync(encounterId, id, itemId);

            if (drop is null)
            {
                return NotFound();
            }

            if (dto.WinnerId?.Length > 0 && drop.WinnerId?.Length > 0)
            {
                ModelState.AddModelError(nameof(dto.WinnerId), "Existing winner must be cleared before setting a new winner.");
                return ValidationProblem();
            }

            drop.AwardedAtUtc = now;
            drop.AwardedBy = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            var killers = await _context.Set<CharacterEncounterKill>()
                .Where(c => c.EncounterKillEncounterId == encounterId && c.EncounterKillRaidId == id)
                .Select(c => c.Character)
                .ToListAsync();

            await _context.Entry(drop).Collection(drop => drop.Passes).LoadAsync();
            drop.Passes.Clear();

            if (dto.WinnerId?.Length > 0)
            {
                var winner = killers.Find(k => k.Id == dto.WinnerId);

                if (winner is null)
                {
                    ModelState.AddModelError(nameof(dto.WinnerId), "Character was not present for the kill.");
                    return ValidationProblem();
                }

                var (winnerPrio, _, _) = await prioCalculator.CalculatePrioAsync(winner.Id, drop.ItemId);

                killers.Remove(winner);

                drop.Winner = winner;
                drop.WinnerId = winner.Id;

                foreach (var killer in killers)
                {
                    var (prio, usable, _) = await prioCalculator.CalculatePrioAsync(killer.Id, drop.ItemId);

                    if (usable && prio.HasValue)
                    {
                        drop.Passes.Add(new DropPass
                        {
                            Character = killer,
                            CharacterId = killer.Id,
                            Drop = drop,
                            RelativePriority = prio.Value - (winnerPrio ?? 0)
                        });
                    }
                }
            }
            else
            {
                drop.Winner = null;
                drop.WinnerId = null;
            }

            await _context.SaveChangesAsync();

            return new EncounterDropDto
            {
                AwardedAt = new DateTimeOffset(drop.AwardedAtUtc, TimeSpan.Zero),
                AwardedBy = drop.AwardedBy,
                ItemId = drop.ItemId,
                WinnerId = drop.WinnerId,
                WinnerName = drop.Winner?.Name
            };
        }
    }
}
