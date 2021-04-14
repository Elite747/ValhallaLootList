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
                .OrderBy(team => team.Name)
                .Select(team => new TeamNameDto
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
            var scope = PrioCalculator.Scope;
            var characterQuery = _context.Characters.AsNoTracking().Where(c => c.TeamId == team.Id);

            await foreach (var member in HelperQueries.GetMembersAsync(_discordService, _serverTimeZone, characterQuery, scope, team.Id, team.Name, isLeader))
            {
                team.Roster.Add(member);
            }

            return team;
        }

        [HttpPost, Authorize(AppPolicies.Administrator)]
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

        [HttpPut("{id:long}"), Authorize(AppPolicies.Administrator)]
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

        [HttpPost("{id:long}/members"), Authorize(AppPolicies.RaidLeader)]
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

            var scope = PrioCalculator.Scope;
            var attendance = await _context.RaidAttendees
                .AsNoTracking()
                .Where(x => !x.IgnoreAttendance && x.CharacterId == character.Id && x.Raid.RaidTeamId == character.TeamId)
                .Select(x => x.Raid.StartedAt.Date)
                .Distinct()
                .OrderByDescending(x => x)
                .Take(scope.ObservedAttendances)
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
                ThisMonthRequiredDonations = scope.RequiredDonationCopper,
                NextMonthRequiredDonations = scope.RequiredDonationCopper,
                Attendance = attendance,
                AttendanceMax = scope.ObservedAttendances
            };

            await foreach (var lootList in _context.CharacterLootLists
                .AsNoTracking()
                .Where(l => l.CharacterId == character.Id)
                .OrderBy(l => l.Phase)
                .Select(l => new { l.MainSpec, l.Status, l.ApprovedBy, l.Phase })
                .AsAsyncEnumerable())
            {
                var lootListDto = new MemberLootListDto
                {
                    Status = lootList.Status,
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

        [HttpPut("{id:long}/members/{characterId:long}"), Authorize(AppPolicies.RaidLeader)]
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

        [HttpDelete("{id:long}/members/{characterId:long}"), Authorize(AppPolicies.RaidLeader)]
        public async Task<IActionResult> DeleteMember(long id, long characterId, [FromServices] IdGen.IIdGenerator<long> idGenerator)
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
            character.MemberStatus = RaidMemberStatus.FullTrial;

            var removal = new TeamRemoval(idGenerator.CreateId())
            {
                Character = character,
                CharacterId = character.Id,
                RemovedAt = _serverTimeZone.TimeZoneNow(),
                Team = team,
                TeamId = team.Id
            };

            await foreach (var attendance in _context.RaidAttendees.AsTracking().Where(a => a.CharacterId == character.Id && a.Raid.RaidTeamId == id && !a.IgnoreAttendance && a.RemovalId == null).AsAsyncEnumerable())
            {
                attendance.IgnoreAttendance = true;
                attendance.IgnoreReason = "Character was removed from the raid team.";
                attendance.Removal = removal;
                attendance.RemovalId = removal.Id;
            }

            await foreach (var donation in _context.Donations.AsTracking().Where(d => d.CharacterId == character.Id && d.RemovalId == null).AsAsyncEnumerable())
            {
                donation.Removal = removal;
                donation.RemovalId = removal.Id;
            }

            await foreach (var pass in _context.DropPasses.AsTracking().Where(p => p.CharacterId == character.Id && p.WonEntryId == null && p.RemovalId == null).AsAsyncEnumerable())
            {
                pass.Removal = removal;
                pass.RemovalId = removal.Id;
            }

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("TeamMemberRemoved", User, props =>
            {
                props["TeamId"] = team.Id.ToString();
                props["TeamName"] = team.Name;
                props["CharacterId"] = character.Id.ToString();
                props["CharacterName"] = character.Name;
                props["RemovalId"] = removal.Id.ToString();
            });

            return Accepted();
        }

        [HttpGet("{id:long}/leaders")]
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

        [HttpPost("{id:long}/leaders/{userId:long}"), Authorize(AppPolicies.Administrator)]
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

        [HttpDelete("{id}/leaders/{userId}"), Authorize(AppPolicies.Administrator)]
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
