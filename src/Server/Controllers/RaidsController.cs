// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;
using static AutoMapper.Internal.ExpressionFactory;

namespace ValhallaLootList.Server.Controllers;

public class RaidsController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;
    private readonly IAuthorizationService _authorizationService;
    private readonly TelemetryClient _telemetry;

    public RaidsController(ApplicationDbContext context, IAuthorizationService authorizationService, TelemetryClient telemetry)
    {
        _context = context;
        _authorizationService = authorizationService;
        _telemetry = telemetry;
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
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new RaidDto
            {
                Id = r.Id,
                Phase = r.Phase,
                StartedAt = r.StartedAt,
                LocksAt = r.LocksAt,
                TeamId = r.RaidTeamId,
                TeamName = r.RaidTeam.Name,
                TeamSize = r.RaidTeam.TeamSize
            })
            .AsAsyncEnumerable();
    }

    [HttpGet("@mine")]
    public IAsyncEnumerable<RaidDto> GetMine()
    {
        var userId = User.GetDiscordId();
        Debug.Assert(userId.HasValue);
        DateTimeOffset end = DateTimeOffset.UtcNow, start = end.AddMonths(-1);

        return _context.Raids
            .AsNoTracking()
            .Where(r => r.StartedAt >= start && r.StartedAt < end && r.Attendees.Any(a => a.Character.OwnerId == userId))
            .OrderByDescending(r => r.StartedAt)
            .Select(r => new RaidDto
            {
                Id = r.Id,
                Phase = r.Phase,
                StartedAt = r.StartedAt,
                LocksAt = r.LocksAt,
                TeamId = r.RaidTeamId,
                TeamName = r.RaidTeam.Name,
                TeamSize = r.RaidTeam.TeamSize
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
                LocksAt = raid.LocksAt,
                Id = raid.Id,
                Phase = raid.Phase,
                TeamId = raid.RaidTeamId,
                TeamName = raid.RaidTeam.Name,
                TeamSize = raid.RaidTeam.TeamSize
            })
            .FirstOrDefaultAsync();

        if (dto is null)
        {
            return NotFound();
        }

        var members = await _context.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.TeamId == dto.TeamId)
            .Select(tm => new { tm.CharacterId, tm.Disenchanter })
            .ToListAsync();

        dto.Attendees = await _context.RaidAttendees
            .AsNoTracking()
            .Where(a => a.RaidId == id)
            .OrderBy(a => a.Character.Name)
            .Select(a => new AttendanceDto
            {
                MainSpec = ((Specializations?)a.Character.CharacterLootLists.FirstOrDefault(ll => ll.Phase == dto.Phase && ll.Size == dto.TeamSize)!.MainSpec).GetValueOrDefault(),
                Standby = a.Standby,
                Disenchanter = members.Any(m => m.Disenchanter && m.CharacterId == a.CharacterId),
                Character = new CharacterDto
                {
                    Class = a.Character.Class,
                    Gender = a.Character.IsFemale ? Gender.Female : Gender.Male,
                    Id = a.CharacterId,
                    Name = a.Character.Name,
                    Race = a.Character.Race,
                    Teams = a.Character.Teams.Select(tm => tm.TeamId).ToList(),
                    Verified = a.Character.VerifiedById.HasValue
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
                TrashIndex = k.TrashIndex
            })
            .ToListAsync();

        var killDictionary = dto.Kills.ToDictionary(k => (k.EncounterId, k.TrashIndex));

        await foreach (var kek in _context.CharacterEncounterKills
            .AsNoTracking()
            .Where(kek => kek.EncounterKillRaidId == id)
            .Select(kek => new
            {
                kek.EncounterKillEncounterId,
                kek.EncounterKillTrashIndex,
                kek.CharacterId
            })
            .AsAsyncEnumerable())
        {
            killDictionary[(kek.EncounterKillEncounterId, kek.EncounterKillTrashIndex)].Characters.Add(kek.CharacterId);
        }

        await foreach (var drop in _context.Drops
            .AsNoTracking()
            .Where(d => d.EncounterKillRaidId == id)
            .Select(d => new
            {
                d.Id,
                d.EncounterKillEncounterId,
                d.EncounterKillTrashIndex,
                d.AwardedAt,
                AwardedBy = (long?)d.AwardedBy,
                WinnerId = (long?)d.WinnerId,
                d.ItemId,
                ItemName = d.Item.Name,
                d.Disenchanted
            })
            .OrderBy(d => d.ItemName)
            .AsAsyncEnumerable())
        {
            killDictionary[(drop.EncounterKillEncounterId, drop.EncounterKillTrashIndex)].Drops.Add(new EncounterDropDto
            {
                Id = drop.Id,
                AwardedAt = drop.AwardedAt,
                AwardedBy = drop.AwardedBy,
                ItemId = drop.ItemId,
                WinnerId = drop.WinnerId,
                Disenchanted = drop.Disenchanted
            });
        }

        return dto;
    }

    [HttpPost, Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult<RaidDto>> Post([FromBody] RaidSubmissionDto dto, [FromServices] TimeZoneInfo realmTimeZoneInfo, [FromServices] IdGen.IIdGenerator<long> idGenerator, [FromServices] IAuthorizationService auth)
    {
        var team = await _context.RaidTeams.FindAsync(dto.TeamId);

        if (team is null)
        {
            ModelState.AddModelError(nameof(dto.TeamId), "Raid Team does not exist.");
            return ValidationProblem();
        }

        var authResult = await auth.AuthorizeAsync(User, team, AppPolicies.LootMaster);

        if (!authResult.Succeeded)
        {
            return Unauthorized();
        }

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

        var raid = new Raid(idGenerator.CreateId())
        {
            Phase = (byte)dto.Phase,
            RaidTeam = team,
            RaidTeamId = team.Id,
            StartedAt = realmTimeZoneInfo.TimeZoneNow()
        };
        raid.LocksAt = raid.StartedAt.AddHours(6);

        var allCharacterIds = dto.Attendees.Concat(dto.Rto).ToList();

        var characters = await _context.Characters.AsTracking().Where(c => allCharacterIds.Contains(c.Id)).Include(c => c.Teams).ToListAsync();

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

                raid.Attendees.Add(attendee);
            }
        }

        for (int i = 0; i < dto.Rto.Count; i++)
        {
            var charId = dto.Rto[i];
            var character = characters.Find(c => c.Id == charId);

            if (character is null)
            {
                ModelState.AddModelError($"{nameof(dto.Rto)}[{i}]", "Character does not exist.");
            }
            else if (!character.Teams.Any(tm => tm.TeamId == team.Id))
            {
                ModelState.AddModelError($"{nameof(dto.Rto)}[{i}]", "Character is not part of this team.");
            }
            else
            {
                raid.Attendees.Add(new()
                {
                    Character = character,
                    CharacterId = character.Id,
                    Raid = raid,
                    RaidId = raid.Id,
                    Standby = true
                });
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        _context.Raids.Add(raid);

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("RaidStarted", User, props =>
        {
            props["TeamId"] = team.Id.ToString();
            props["TeamName"] = team.Name;
            props["Phase"] = raid.Phase.ToString();
        });

        var members = await _context.TeamMembers
            .AsNoTracking()
            .Where(tm => tm.TeamId == dto.TeamId)
            .Select(tm => new { tm.CharacterId, tm.Disenchanter })
            .ToListAsync();

        return CreatedAtAction(nameof(Get), new { id = raid.Id }, new RaidDto
        {
            Attendees = raid.Attendees.Select(a => new AttendanceDto
            {
                MainSpec = a.Character.CharacterLootLists.FirstOrDefault(ll => ll.Phase == dto.Phase && ll.Size == team.TeamSize)?.MainSpec ?? Specializations.None,
                Standby = a.Standby,
                Disenchanter = members.Any(m => m.Disenchanter && m.CharacterId == a.CharacterId),
                Character = new CharacterDto
                {
                    Id = a.CharacterId,
                    Class = a.Character.Class,
                    Gender = a.Character.IsFemale ? Gender.Female : Gender.Male,
                    Name = a.Character.Name,
                    Race = a.Character.Race,
                    Teams = a.Character.Teams.Select(tm => tm.TeamId).ToList(),
                    Verified = a.Character.VerifiedById.HasValue
                }
            }).ToList(),
            StartedAt = raid.StartedAt,
            LocksAt = raid.LocksAt,
            Id = raid.Id,
            Phase = raid.Phase,
            TeamId = team.Id,
            TeamName = team.Name,
            TeamSize = team.TeamSize
        });
    }

    [HttpDelete("{id:long}"), Authorize(AppPolicies.LootMasterOrAdmin)]
    public async Task<ActionResult> Delete(long id)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMasterOrAdmin);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt && !User.IsAdmin())
        {
            return Problem("Can't delete a locked raid.");
        }

        if (await _context.EncounterKills.CountAsync(ek => ek.RaidId == id) > 0)
        {
            return Problem("Can't delete a raid with recorded kills.");
        }

        _context.Raids.Remove(raid);

        await _context.SaveChangesAsync();

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        _telemetry.TrackEvent("RaidDeleted", User, props =>
        {
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
            props["Phase"] = raid.Phase.ToString();
        });

        return Ok();
    }

    [HttpPost("{id:long}/Unlock"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<UnlockResponse>> Unlock(long id, [FromServices] TimeZoneInfo realmTimeZoneInfo)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        raid.LocksAt = realmTimeZoneInfo.TimeZoneNow().AddMinutes(10);

        await _context.SaveChangesAsync();

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        _telemetry.TrackEvent("RaidUnlocked", User, props =>
        {
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
        });

        return new UnlockResponse { LocksAt = raid.LocksAt };
    }

    [HttpPost("{id:long}/Attendees"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult<AttendanceDto>> PostAttendee(long id, [FromBody] AttendeeSubmissionDto dto)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMaster);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt)
        {
            return Problem("Can't alter a locked raid.");
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
            Raid = raid,
            RaidId = raid.Id,
            Standby = dto.Standby
        };

        var characterTeams = await _context.TeamMembers.Where(tm => tm.CharacterId == character.Id).Select(tm => tm.TeamId).ToListAsync();

        _context.RaidAttendees.Add(attendee);

        await _context.SaveChangesAsync();

        var team = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => new { t.Name, t.TeamSize })
            .FirstAsync();

        _telemetry.TrackEvent("AttendeeAdded", User, props =>
        {
            props["RaidId"] = raid.Id.ToString();
            props["CharacterId"] = character.Id.ToString();
            props["CharacterName"] = character.Name;
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = team.Name;
            props["Phase"] = raid.Phase.ToString();
        });

        var spec = await _context.CharacterLootLists
            .AsNoTracking()
            .Where(ll => ll.Phase == raid.Phase && ll.Size == team.TeamSize && ll.CharacterId == character.Id)
            .Select(ll => ll.MainSpec)
            .FirstOrDefaultAsync();

        return new AttendanceDto
        {
            MainSpec = spec,
            Standby = attendee.Standby,
            Disenchanter = await _context.TeamMembers.CountAsync(tm => tm.CharacterId == character.Id && tm.TeamId == raid.RaidTeamId && tm.Disenchanter) > 0,
            Character = new CharacterDto
            {
                Class = character.Class,
                Gender = character.IsFemale ? Gender.Female : Gender.Male,
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Teams = characterTeams,
                Verified = character.VerifiedById.HasValue
            }
        };
    }

    [HttpPut("{id:long}/Attendees/{characterId:long}"), Authorize(AppPolicies.LootMasterOrAdmin)]
    public async Task<ActionResult<AttendanceDto>> PutAttendee(long id, long characterId, [FromBody] UpdateAttendanceSubmissionDto dto)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMasterOrAdmin);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt && !User.IsAdmin())
        {
            return Problem("Can't alter a locked raid.");
        }

        var attendee = await _context.RaidAttendees.FindAsync(characterId, id);

        if (attendee is null)
        {
            return NotFound();
        }

        attendee.Standby = dto.Standby;

        await _context.SaveChangesAsync();

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        var character = await _context.Characters
            .AsNoTracking()
            .Where(c => c.Id == characterId)
            .Select(c => new { c.Id, c.Name, c.Class, Gender = c.IsFemale ? Gender.Female : Gender.Male, c.Race, Teams = c.Teams.Select(t => t.TeamId).ToList(), c.VerifiedById })
            .FirstAsync();

        _telemetry.TrackEvent("AttendeeUpdated", User, props =>
        {
            props["RaidId"] = raid.Id.ToString();
            props["CharacterId"] = character.Id.ToString();
            props["CharacterName"] = character.Name;
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
            props["Phase"] = raid.Phase.ToString();
        });

        return new AttendanceDto
        {
            Standby = attendee.Standby,
            Disenchanter = await _context.TeamMembers.CountAsync(tm => tm.CharacterId == character.Id && tm.TeamId == raid.RaidTeamId && tm.Disenchanter) > 0,
            Character = new CharacterDto
            {
                Class = character.Class,
                Gender = character.Gender,
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Teams = character.Teams,
                Verified = character.VerifiedById.HasValue
            }
        };
    }

    [HttpDelete("{id:long}/Attendees/{characterId:long}"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult> DeleteAttendee(long id, long characterId)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMaster);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt)
        {
            return Problem("Can't alter a locked raid.");
        }

        var attendee = await _context.RaidAttendees.FindAsync(characterId, id);

        if (attendee is null)
        {
            return NotFound();
        }

        _context.RaidAttendees.Remove(attendee);

        await _context.SaveChangesAsync();

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        var character = await _context.Characters
            .AsNoTracking()
            .Where(c => c.Id == characterId)
            .Select(c => new { c.Id, c.Name })
            .FirstAsync();

        _telemetry.TrackEvent("AttendeeDeleted", User, props =>
        {
            props["RaidId"] = raid.Id.ToString();
            props["CharacterId"] = character.Id.ToString();
            props["CharacterName"] = character.Name;
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
            props["Phase"] = raid.Phase.ToString();
        });

        return Ok();
    }

    [HttpPost("{id:long}/Kills"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult<EncounterKillDto>> PostKill(long id, [FromBody] KillSubmissionDto dto, [FromServices] TimeZoneInfo realmTimeZoneInfo, [FromServices] IdGen.IIdGenerator<long> idGenerator, [FromServices] MessageSender messageSender)
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

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMaster);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt)
        {
            return Problem("Can't alter a locked raid.");
        }

        var encounter = await _context.Encounters
            .AsTracking()
            .Where(e => e.Id == dto.EncounterId)
            .Select(e => new { e.Id, e.Name, e.Instance.Phase, e.Index, Items = e.Items.Select(i => i.ItemId).ToList() })
            .FirstOrDefaultAsync();

        if (encounter is null)
        {
            ModelState.AddModelError(nameof(dto.EncounterId), "Encounter does not exist.");
            return ValidationProblem();
        }
        if (!await _context.PhaseActiveAsync(encounter.Phase))
        {
            return Problem("Phase is not yet active.");
        }

        var existingKillIndex = await _context.EncounterKills
            .Where(ek => ek.RaidId == id && ek.EncounterId == dto.EncounterId)
            .OrderByDescending(ek => ek.TrashIndex)
            .Select(ek => (byte?)ek.TrashIndex)
            .FirstOrDefaultAsync();

        if (encounter.Index >= 0 && existingKillIndex.HasValue)
        {
            return Problem("That boss already has a recorded kill for this raid.");
        }

        var kill = new EncounterKill
        {
            KilledAt = realmTimeZoneInfo.TimeZoneNow(),
            EncounterId = encounter.Id,
            Raid = raid,
            RaidId = raid.Id,
            TrashIndex = existingKillIndex.HasValue ? (byte)(existingKillIndex.Value + 1) : default
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
                    EncounterKillRaidId = kill.RaidId,
                    EncounterKillTrashIndex = kill.TrashIndex
                });
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        var items = await _context.Items.AsTracking().Where(i => dto.Drops.Contains(i.Id)).ToListAsync();
        var drops = new List<(uint, string, string?)>();

        for (int i = 0; i < dto.Drops.Count; i++)
        {
            var itemId = dto.Drops[i];
            var item = items.Find(c => c.Id == itemId);

            if (item is null)
            {
                ModelState.AddModelError($"{nameof(dto.Drops)}[{i}]", "Item does not exist.");
            }
            else if (!encounter.Items.Contains(item.Id))
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
                    EncounterKillTrashIndex = kill.TrashIndex,
                    Item = item,
                    ItemId = item.Id
                });
                drops.Add((itemId, item.Name, null));
            }
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        _context.EncounterKills.Add(kill);

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        await _context.SaveChangesAsync();

        await messageSender.SendKillMessageAsync(kill.RaidId, kill.EncounterId, kill.TrashIndex);

        _telemetry.TrackEvent("KillAdded", User, props =>
        {
            props["RaidId"] = raid.Id.ToString();
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
            props["Phase"] = raid.Phase.ToString();
            props["Encounter"] = encounter.Id;
        });

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
                Disenchanted = d.Disenchanted
            }).ToList(),
            EncounterId = kill.EncounterId,
            EncounterName = encounter.Name,
            TrashIndex = kill.TrashIndex
        };
    }

    [HttpDelete("{id:long}/Kills/{encounterId}/{trashIndex}"), Authorize(AppPolicies.LootMaster)]
    public async Task<ActionResult> DeleteKill(long id, string encounterId, byte trashIndex, [FromServices] MessageSender messageSender)
    {
        var raid = await _context.Raids.FindAsync(id);

        if (raid is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, raid.RaidTeamId, AppPolicies.LootMaster);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        if (DateTimeOffset.UtcNow > raid.LocksAt)
        {
            return Problem("Can't alter a locked raid.");
        }

        var kill = await _context.EncounterKills.FindAsync(encounterId, id, trashIndex);

        if (kill is null)
        {
            return NotFound();
        }

        var drops = await _context.Drops.AsTracking().Where(drop => drop.EncounterKillEncounterId == encounterId && drop.EncounterKillRaidId == id && drop.EncounterKillTrashIndex == trashIndex).ToListAsync();

        if (drops.Any(d => d.WinnerId is not null))
        {
            return Problem("Can't delete a kill that has items already awarded.", statusCode: 400);
        }

        _context.Drops.RemoveRange(drops);
        _context.EncounterKills.Remove(kill);

        await _context.SaveChangesAsync();

        await messageSender.DeleteKillMessageAsync(kill.DiscordMessageId);

        var teamName = await _context.RaidTeams
            .AsNoTracking()
            .Where(t => t.Id == raid.RaidTeamId)
            .Select(t => t.Name)
            .FirstAsync();

        _telemetry.TrackEvent("KillDeleted", User, props =>
        {
            props["RaidId"] = raid.Id.ToString();
            props["TeamId"] = raid.RaidTeamId.ToString();
            props["TeamName"] = teamName;
            props["Phase"] = raid.Phase.ToString();
            props["Encounter"] = kill.EncounterId;
        });

        return Ok();
    }
}
