// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class TeamsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public TeamsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IAsyncEnumerable<string> Get()
        {
            return _context.RaidTeams
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .AsAsyncEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TeamDto>> Get(string id)
        {
            var team = await _context.RaidTeams
                .AsNoTracking()
                .Where(team => team.Id == id || team.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
                .Select(team => new TeamDto
                {
                    Id = team.Id,
                    Name = team.Name,
                    Schedules = team.Schedules.Select(s => new ScheduleDto
                    {
                        Day = s.Day,
                        RealmTimeStart = s.RealmTimeStart,
                        Duration = s.Duration
                    }).ToList()
                })
                .AsSingleQuery()
                .FirstOrDefaultAsync();

            if (team is null)
            {
                return NotFound();
            }

            var currentPhase = await _context.GetCurrentPhaseAsync();

            await foreach (var character in _context.Characters
                .AsNoTracking()
                .Where(c => c.TeamId == team.Id)
                .Select(c => new
                {
                    c.Class,
                    c.Id,
                    c.Name,
                    c.Race,
                    c.IsMale,
                    c.MemberStatus,
                    CurrentLootList = c.CharacterLootLists.Where(l => l.Phase == currentPhase).Select(l => new { l.MainSpec, l.OffSpec, l.Phase }).FirstOrDefault()
                })
                .AsAsyncEnumerable())
            {
                team.Roster.Add(new TeamCharacterDto
                {
                    Class = character.Class,
                    CurrentPhaseMainspec = character.CurrentLootList?.MainSpec,
                    CurrentPhaseOffspec = character.CurrentLootList?.OffSpec,
                    Gender = character.IsMale ? Gender.Male : Gender.Female,
                    Id = character.Id,
                    MemberStatus = character.MemberStatus,
                    Name = character.Name,
                    Race = character.Race
                });
            }

            return team;
        }

        [HttpPost, Authorize(AppRoles.Administrator)]
        public async Task<ActionResult<CharacterDto>> Post([FromBody] TeamSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            Debug.Assert(dto.Name?.Length > 1);

            if (dto.Schedules.Count == 0)
            {
                ModelState.AddModelError(nameof(dto.Schedules), "At least one raid day schedule must be entered.");
            }

            var team = new RaidTeam
            {
                Name = dto.Name
            };

            foreach (var scheduleDto in dto.Schedules)
            {
                Debug.Assert(scheduleDto.Day.HasValue);
                Debug.Assert(scheduleDto.StartTime.HasValue);
                Debug.Assert(scheduleDto.Duration.HasValue);

                team.Schedules.Add(new RaidTeamSchedule
                {
                    Day = scheduleDto.Day.Value,
                    Duration = TimeSpan.FromHours(scheduleDto.Duration.Value),
                    RealmTimeStart = scheduleDto.StartTime.Value,
                    RaidTeam = team
                });
            }

            _context.RaidTeams.Add(team);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = team.Id }, new TeamDto
            {
                Id = team.Id,
                Name = team.Name,
                Schedules = team.Schedules.Select(s => new ScheduleDto
                {
                    Day = s.Day,
                    RealmTimeStart = s.RealmTimeStart,
                    Duration = s.Duration
                }).ToList()
            });
        }

        [HttpPost("{id}/members"), Authorize(AppRoles.RaidLeader)]
        public async Task<ActionResult<TeamCharacterDto>> PostMember(string id, [FromBody] AddTeamMemberDto dto)
        {
            if (await _context.RaidTeams.AsNoTracking().CountAsync(t => t.Id == id) == 0)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(dto.CharacterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.TeamId?.Length > 0 && character.TeamId != id)
            {
                return Problem("Character is already a part of another team.");
            }

            character.TeamId = id;
            character.MemberStatus = dto.MemberStatus;

            await _context.SaveChangesAsync();

            var currentPhase = await _context.GetCurrentPhaseAsync();
            var characterLootList = await _context.CharacterLootLists.Where(l => l.Phase == currentPhase).Select(l => new { l.MainSpec, l.OffSpec }).FirstOrDefaultAsync();

            return Ok(new TeamCharacterDto
            {
                Class = character.Class,
                CurrentPhaseMainspec = characterLootList?.MainSpec,
                CurrentPhaseOffspec = characterLootList?.OffSpec,
                Gender = character.IsMale ? Gender.Male : Gender.Female,
                Id = character.Id,
                MemberStatus = character.MemberStatus,
                Name = character.Name,
                Race = character.Race
            });
        }

        [HttpPut("{id}/members/{characterId}"), Authorize(AppRoles.RaidLeader)]
        public async Task<IActionResult> PutMember(string id, string characterId, [FromBody] UpdateTeamMemberDto dto)
        {
            if (await _context.RaidTeams.AsNoTracking().CountAsync(t => t.Id == id) == 0)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.TeamId != id)
            {
                return Problem("Character is not assigned to this team.");
            }

            character.MemberStatus = dto.MemberStatus;

            await _context.SaveChangesAsync();

            return Accepted();
        }

        [HttpDelete("{id}/members/{characterId}"), Authorize(AppRoles.RaidLeader)]
        public async Task<IActionResult> DeleteMember(string id, string characterId)
        {
            if (await _context.RaidTeams.AsNoTracking().CountAsync(t => t.Id == id) == 0)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(characterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.TeamId != id)
            {
                return Problem("Character is not assigned to this team.");
            }

            character.TeamId = null;

            await foreach (var attendance in _context.RaidAttendees.AsTracking().Where(a => a.CharacterId == character.Id && a.Raid.RaidTeamId == id && !a.IgnoreAttendance).AsAsyncEnumerable())
            {
                attendance.IgnoreAttendance = true;
                attendance.IgnoreReason = "Character was removed from the raid team.";
            }

            await _context.SaveChangesAsync();

            return Accepted();
        }
    }
}
