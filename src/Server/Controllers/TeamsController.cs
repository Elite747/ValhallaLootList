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
        private readonly TimeZoneInfo _serverTimeZone;
        private readonly TelemetryClient _telemetry;

        public TeamsController(ApplicationDbContext context, TimeZoneInfo serverTimeZone, TelemetryClient telemetry)
        {
            _context = context;
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
            var scope = await _context.GetCurrentPriorityScopeAsync();
            var characterQuery = _context.Characters.AsNoTracking().Where(c => c.TeamId == team.Id);

            foreach (var member in await HelperQueries.GetMembersAsync(_context, _serverTimeZone, characterQuery, scope, team.Id, team.Name, isLeader))
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
                    if (long.TryParse(otherClaim, out var characterId))
                    {
                        characterIds.Add(characterId);
                    }
                }

                var existingCharacterName = await _context.Characters
                    .AsNoTracking()
                    .Where(c => characterIds.Contains(c.Id) && c.TeamId == id)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();

                if (existingCharacterName?.Length > 0)
                {
                    return Problem($"The owner of this character is already on this team as {existingCharacterName}.");
                }
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

            var scope = await _context.GetCurrentPriorityScopeAsync();
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
                JoinedAt = character.JoinedTeamAt,
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
                .Select(l => new
                {
                    l.MainSpec,
                    l.Status,
                    l.ApprovedBy,
                    ApprovedByName = l.ApprovedBy.HasValue ? _context.Users.Where(u => u.Id == l.ApprovedBy).Select(u => u.UserName).FirstOrDefault() : null,
                    l.Phase
                })
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
                    lootListDto.ApprovedBy = lootList.ApprovedByName;
                }
                else
                {
                    lootListDto.Approved = false;
                }

                returnDto.LootLists.Add(lootListDto);
            }

            var now = _serverTimeZone.TimeZoneNow();
            var donationMatrix = await _context.GetDonationMatrixAsync(d => d.CharacterId == character.Id, scope);

            returnDto.DonatedThisMonth = donationMatrix.GetCreditForMonth(returnDto.Character.Id, now);
            returnDto.DonatedNextMonth = donationMatrix.GetDonatedDuringMonth(returnDto.Character.Id, now);

            if (returnDto.DonatedThisMonth > scope.RequiredDonationCopper)
            {
                returnDto.DonatedNextMonth += returnDto.DonatedThisMonth - scope.RequiredDonationCopper;
            }

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

        [HttpDelete("{id:long}/members/{characterId:long}")]
        public async Task<IActionResult> DeleteMember(long id, long characterId, [FromServices] IdGen.IIdGenerator<long> idGenerator, [FromServices] IAuthorizationService authService)
        {
            var auth = await authService.AuthorizeAsync(User, id, AppPolicies.RaidLeader);

            if (!auth.Succeeded)
            {
                auth = await authService.AuthorizeAsync(User, characterId, AppPolicies.CharacterOwner);
                if (!auth.Succeeded)
                {
                    return Unauthorized();
                }
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

            var removal = new TeamRemoval(idGenerator.CreateId())
            {
                Character = character,
                CharacterId = character.Id,
                RemovedAt = _serverTimeZone.TimeZoneNow(),
                JoinedAt = character.JoinedTeamAt,
                Team = team,
                TeamId = team.Id
            };

            character.TeamId = null;
            character.MemberStatus = RaidMemberStatus.FullTrial;
            character.JoinedTeamAt = default;

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

        [HttpGet("{id:long}/leaders"), Authorize]
        public async IAsyncEnumerable<GuildMemberDto> GetLeaders(string id, [FromServices] DiscordClientProvider dcp)
        {
            await foreach (var userId in _context.UserClaims
                .AsNoTracking()
                .Where(c => c.ClaimType == AppClaimTypes.RaidLeader && c.ClaimValue == id)
                .Select(c => c.UserId)
                .AsAsyncEnumerable())
            {
                var member = await dcp.GetMemberAsync(userId);

                if (member is not null)
                {
                    yield return dcp.CreateDto(member);
                }
            }
        }

        [HttpPost("{id:long}/leaders/{userId:long}"), Authorize(AppPolicies.Administrator)]
        public async Task<ActionResult<GuildMemberDto>> PostLeader(long id, long userId, [FromServices] DiscordClientProvider dcp)
        {
            var team = await _context.RaidTeams.FindAsync(id);

            if (team is null)
            {
                return NotFound();
            }

            var leader = await dcp.GetMemberAsync(userId);

            if (leader is null)
            {
                return Problem("Couldn't locate discord user.");
            }

            if (!dcp.HasRaidLeaderRole(leader) && !dcp.HasLootMasterRole(leader))
            {
                // TODO: should we just assign the role here instead of failing?
                return Problem($"{leader.DisplayName} does not have a raid leader or loot master role assigned.");
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
                props["LeaderName"] = leader.DisplayName;
            });

            return dcp.CreateDto(leader);
        }

        [HttpDelete("{id:long}/leaders/{userId:long}"), Authorize(AppPolicies.Administrator)]
        public async Task<IActionResult> DeleteLeader(long id, long userId)
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

            var idString = id.ToString();

            var claim = await _context.UserClaims.FirstOrDefaultAsync(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.RaidLeader && claim.ClaimValue == idString);

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
