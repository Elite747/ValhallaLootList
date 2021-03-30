// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    public class TeamsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly DiscordService _discordService;
        private readonly TimeZoneInfo _serverTimeZone;
        private readonly TelemetryClient _telemetry;

        public TeamsController(ApplicationDbContext context, DiscordService discordService, TimeZoneInfo serverTimeZone, TelemetryClient telemetry)
        {
            _context = context;
            _discordService = discordService;
            _serverTimeZone = serverTimeZone;
            _telemetry = telemetry;
        }

        [HttpGet]
        public IAsyncEnumerable<TeamNameDto> Get()
        {
            return _context.RaidTeams
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new TeamNameDto { Id = c.Id, Name = c.Name })
                .AsAsyncEnumerable();
        }

        [HttpGet("{id:long}")]
        public Task<ActionResult<TeamDto>> Get(long id)
        {
            return GetTeamAsync(team => team.Id == id);
        }

        [HttpGet("ByName/{name}")]
        public Task<ActionResult<TeamDto>> Get(string name)
        {
            return GetTeamAsync(team => team.Name == name);
        }

        private async Task<ActionResult<TeamDto>> GetTeamAsync(Expression<Func<RaidTeam, bool>> match)
        {
            var team = await _context.RaidTeams
                .AsNoTracking()
                .Where(match)
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

            bool isLeader = await _context.IsLeaderOf(User, team.Id);

            var now = _serverTimeZone.TimeZoneNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var nextMonth = thisMonth.AddMonths(1);

            await foreach (var character in _context.Characters
                .AsNoTracking()
                .Where(c => c.TeamId == team.Id)
                .Select(c => new
                {
                    c.Class,
                    c.Id,
                    c.Name,
                    c.Race,
                    c.IsFemale,
                    c.MemberStatus,
                    Verified = c.VerifiedById.HasValue,
                    LootLists = c.CharacterLootLists.Select(l => new { l.MainSpec, l.ApprovedBy, l.Locked, l.Phase }).ToList(),
                    DonatedThisMonth = c.Donations.Where(d => d.Month == thisMonth.Month && d.Year == thisMonth.Year).Sum(d => (long)d.CopperAmount),
                    DonatedNextMonth = c.Donations.Where(d => d.Month == nextMonth.Month && d.Year == nextMonth.Year).Sum(d => (long)d.CopperAmount),
                    Attendance = c.Attendances.Where(x => !x.IgnoreAttendance && x.Raid.RaidTeamId == team.Id)
                        .Select(x => x.Raid.StartedAt.Date)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .Take(PrioCalculator.ObservedRaidsForAttendance)
                        .Count()
                })
                .AsSingleQuery()
                .AsAsyncEnumerable())
            {
                var memberDto = new MemberDto
                {
                    Character = new()
                    {
                        Class = character.Class,
                        Gender = character.IsFemale ? Gender.Female : Gender.Male,
                        Id = character.Id,
                        Name = character.Name,
                        Race = character.Race,
                        TeamId = team.Id,
                        TeamName = team.Name
                    },
                    Status = character.MemberStatus,
                    Verified = character.Verified,
                    DonatedThisMonth = character.DonatedThisMonth,
                    DonatedNextMonth = character.DonatedNextMonth,
                    ThisMonthRequiredDonations = PrioCalculator.CopperForDonationPrio,
                    NextMonthRequiredDonations = PrioCalculator.CopperForDonationPrio,
                    Attendance = character.Attendance,
                    AttendanceMax = PrioCalculator.ObservedRaidsForAttendance,
                    AttendanceBonus = PrioCalculator.CalculateAttendanceBonus(character.Attendance)
                };

                foreach (var lootList in character.LootLists.OrderBy(ll => ll.Phase))
                {
                    var lootListDto = new MemberLootListDto
                    {
                        Locked = lootList.Locked,
                        MainSpec = lootList.MainSpec,
                        Phase = lootList.Phase
                    };

                    if (isLeader)
                    {
                        if (lootList.ApprovedBy.HasValue)
                        {
                            lootListDto.Approved = true;
                            lootListDto.ApprovedBy = await _discordService.GetGuildMemberDtoAsync(lootList.ApprovedBy);
                        }
                        else
                        {
                            lootListDto.Approved = false;
                        }
                    }

                    memberDto.LootLists.Add(lootListDto);
                }

                team.Roster.Add(memberDto);
            }

            return team;
        }

        [HttpPost, Authorize(AppRoles.Administrator)]
        public async Task<ActionResult<TeamDto>> Post([FromBody] TeamSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
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

            var team = new RaidTeam(idGenerator.CreateId())
            {
                Name = dto.Name
            };

            foreach (var scheduleDto in dto.Schedules)
            {
                Debug.Assert(scheduleDto.Day.HasValue);
                Debug.Assert(scheduleDto.StartTime.HasValue);
                Debug.Assert(scheduleDto.Duration.HasValue);

                team.Schedules.Add(new RaidTeamSchedule(idGenerator.CreateId())
                {
                    Day = scheduleDto.Day.Value,
                    Duration = TimeSpan.FromHours(scheduleDto.Duration.Value),
                    RealmTimeStart = scheduleDto.StartTime.Value,
                    RaidTeam = team
                });
            }

            _context.RaidTeams.Add(team);

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamAdded", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
            });

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

        [HttpPut("{id:long}"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult<TeamDto>> Put(long id, [FromBody] TeamSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
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

            var team = await _context.RaidTeams.FindAsync(id);

            team.Name = dto.Name;

            var schedules = await _context.RaidTeamSchedules.AsTracking().Where(s => s.RaidTeam == team).OrderBy(s => s.Id).ToListAsync();

            for (int i = 0, count = Math.Max(schedules.Count, dto.Schedules.Count); i < count; i++)
            {
                switch ((i < schedules.Count, i < dto.Schedules.Count))
                {
                    case (true, true):
                        var scheduleDto = dto.Schedules[i];
                        Debug.Assert(scheduleDto.Day.HasValue);
                        Debug.Assert(scheduleDto.StartTime.HasValue);
                        Debug.Assert(scheduleDto.Duration.HasValue);
                        schedules[i].Day = scheduleDto.Day.Value;
                        schedules[i].Duration = TimeSpan.FromHours(scheduleDto.Duration.Value);
                        schedules[i].RealmTimeStart = scheduleDto.StartTime.Value;
                        break;
                    case (true, false):
                        _context.RaidTeamSchedules.Remove(schedules[i]);
                        break;
                    case (false, true):
                        scheduleDto = dto.Schedules[i];
                        Debug.Assert(scheduleDto.Day.HasValue);
                        Debug.Assert(scheduleDto.StartTime.HasValue);
                        Debug.Assert(scheduleDto.Duration.HasValue);
                        _context.RaidTeamSchedules.Add(new(idGenerator.CreateId())
                        {
                            Day = scheduleDto.Day.Value,
                            Duration = TimeSpan.FromHours(scheduleDto.Duration.Value),
                            RealmTimeStart = scheduleDto.StartTime.Value,
                            RaidTeam = team
                        });
                        break;
                }
            }

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamUpdated", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
            });

            return Ok(new TeamDto
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

        [HttpPost("{id:long}/members"), Authorize(AppRoles.RaidLeader)]
        public async Task<ActionResult<MemberDto>> PostMember(long id, [FromBody] AddTeamMemberDto dto)
        {
            if (!await _context.IsLeaderOf(User, id))
            {
                return Unauthorized();
            }

            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
            {
                return NotFound();
            }

            var character = await _context.Characters.FindAsync(dto.CharacterId);

            if (character is null)
            {
                return NotFound();
            }

            if (character.TeamId.HasValue && character.TeamId != id)
            {
                return Problem("Character is already a part of another team.");
            }

            character.TeamId = id;
            character.MemberStatus = dto.MemberStatus;

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamMemberAdded", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
            });

            var attendance = await _context.RaidAttendees
                .AsNoTracking()
                .Where(x => !x.IgnoreAttendance && x.CharacterId == character.Id && x.Raid.RaidTeamId == character.TeamId)
                .Select(x => x.Raid.StartedAt.Date)
                .Distinct()
                .OrderByDescending(x => x)
                .Take(PrioCalculator.ObservedRaidsForAttendance)
                .CountAsync();

            var returnDto = new MemberDto
            {
                Character = new()
                {
                    Class = character.Class,
                    Gender = character.IsFemale ? Gender.Female : Gender.Male,
                    Id = character.Id,
                    Name = character.Name,
                    Race = character.Race,
                    TeamId = id,
                    TeamName = null // TODO
                },
                Status = character.MemberStatus,
                Verified = character.VerifiedById.HasValue,
                ThisMonthRequiredDonations = PrioCalculator.CopperForDonationPrio,
                NextMonthRequiredDonations = PrioCalculator.CopperForDonationPrio,
                Attendance = attendance,
                AttendanceMax = PrioCalculator.ObservedRaidsForAttendance,
                AttendanceBonus = PrioCalculator.CalculateAttendanceBonus(attendance)
            };

            await foreach (var lootList in _context.CharacterLootLists
                .AsNoTracking()
                .Where(l => l.CharacterId == character.Id)
                .OrderBy(l => l.Phase)
                .Select(l => new { l.MainSpec, l.Locked, l.ApprovedBy, l.Phase })
                .AsAsyncEnumerable())
            {
                var lootListDto = new MemberLootListDto
                {
                    Locked = lootList.Locked,
                    MainSpec = lootList.MainSpec,
                    Phase = lootList.Phase
                };

                if (lootList.ApprovedBy.HasValue)
                {
                    lootListDto.Approved = true;
                    lootListDto.ApprovedBy = await _discordService.GetGuildMemberDtoAsync(lootList.ApprovedBy);
                }
                else
                {
                    lootListDto.Approved = false;
                }

                returnDto.LootLists.Add(lootListDto);
            }

            var now = _serverTimeZone.TimeZoneNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var nextMonth = thisMonth.AddMonths(1);

            returnDto.DonatedThisMonth = await _context.Donations
                .AsNoTracking()
                .Where(d => d.CharacterId == character.Id && d.Month == thisMonth.Month && d.Year == thisMonth.Year)
                .SumAsync(d => (long)d.CopperAmount);

            returnDto.DonatedNextMonth = await _context.Donations
                .AsNoTracking()
                .Where(d => d.CharacterId == character.Id && nextMonth.Month == now.Month && d.Year == nextMonth.Year)
                .SumAsync(d => (long)d.CopperAmount);

            return returnDto;
        }

        [HttpPut("{id:long}/members/{characterId:long}"), Authorize(AppRoles.RaidLeader)]
        public async Task<IActionResult> PutMember(long id, long characterId, [FromBody] UpdateTeamMemberDto dto)
        {
            if (dto.MemberStatus < RaidMemberStatus.Member || dto.MemberStatus > RaidMemberStatus.FullTrial)
            {
                ModelState.AddModelError(nameof(dto.MemberStatus), "Unknown member status.");
                return ValidationProblem();
            }

            if (!await _context.IsLeaderOf(User, id))
            {
                return Unauthorized();
            }

            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
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

            _telemetry.TrackEvent("TeamMemberStatusUpdated", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
            });

            return Accepted();
        }

        [HttpDelete("{id:long}/members/{characterId:long}"), Authorize(AppRoles.RaidLeader)]
        public async Task<IActionResult> DeleteMember(long id, long characterId)
        {
            if (!await _context.IsLeaderOf(User, id))
            {
                return Unauthorized();
            }

            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
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

            _telemetry.TrackEvent("TeamMemberRemoved", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
            });

            return Accepted();
        }

        [HttpGet("{id:long}/leaders"), Authorize(AppRoles.Administrator)]
        public async IAsyncEnumerable<GuildMemberDto> GetLeaders(string id, [FromServices] DiscordService discordService)
        {
            await foreach (var userId in _context.UserClaims
                .AsNoTracking()
                .Where(claim => claim.ClaimValue == id && claim.ClaimType == AppClaimTypes.RaidLeader)
                .Select(claim => claim.UserId)
                .AsAsyncEnumerable())
            {
                var guildMember = await discordService.GetGuildMemberDtoAsync(userId);

                if (guildMember is not null)
                {
                    yield return guildMember;
                }
            }
        }

        [HttpPost("{id:long}/leaders/{userId:long}"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult<GuildMemberDto>> PostLeader(long id, long userId, [FromServices] DiscordService discordService)
        {
            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
            {
                return NotFound();
            }

            var leader = await _context.Users.FindAsync(userId);

            if (leader is null)
            {
                return NotFound();
            }

            var guildMember = await discordService.GetGuildMemberDtoAsync(userId);

            if (guildMember is null)
            {
                return Problem("Couldn't locate the corresponding discord user.");
            }

            var idString = id.ToString();
            if (await _context.UserClaims.CountAsync(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.RaidLeader && claim.ClaimValue == idString) > 0)
            {
                return Problem("User is already a leader of this team.");
            }

            _context.UserClaims.Add(new IdentityUserClaim<long> { UserId = userId, ClaimType = AppClaimTypes.RaidLeader, ClaimValue = idString });
            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamLeaderAdded", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["LeaderId"] = leader.Id.ToString();
                props["LeaderName"] = leader.UserName;
            });

            return guildMember;
        }

        [HttpDelete("{id}/leaders/{userId}"), Authorize(AppRoles.Administrator)]
        public async Task<IActionResult> DeleteLeader(string id, long userId)
        {
            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
            {
                return NotFound();
            }

            var leader = await _context.Users.FindAsync(userId);

            if (leader is null)
            {
                return NotFound();
            }

            var claim = await _context.UserClaims.FirstOrDefaultAsync(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.RaidLeader && claim.ClaimValue == id);

            if (claim is null) return NotFound();

            _context.UserClaims.Remove(claim);
            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamLeaderRemoved", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["LeaderId"] = leader.Id.ToString();
                props["LeaderName"] = leader.UserName;
            });

            return Ok();
        }
    }
}
