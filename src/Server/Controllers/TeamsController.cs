// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers;

public class TeamsController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;
    private readonly TimeZoneInfo _serverTimeZone;
    private readonly TelemetryClient _telemetry;
    private readonly IAuthorizationService _authorizationService;

    public TeamsController(ApplicationDbContext context, TimeZoneInfo serverTimeZone, TelemetryClient telemetry, IAuthorizationService authorizationService)
    {
        _context = context;
        _serverTimeZone = serverTimeZone;
        _telemetry = telemetry;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public IAsyncEnumerable<TeamNameDto> Get()
    {
        return _context.RaidTeams
            .AsNoTracking()
            .OrderByDescending(team => team.TeamSize)
            .ThenBy(team => team.Name)
            .Select(team => new TeamNameDto
            {
                Id = team.Id,
                Name = team.Name,
                Inactive = team.Inactive,
                Size = team.TeamSize,
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
                Size = team.TeamSize,
                Inactive = team.Inactive,
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

        var leaderResult = await _authorizationService.AuthorizeAsync(User, team, AppPolicies.Leadership);

        foreach (var member in await HelperQueries.GetMembersAsync(_context, _serverTimeZone, team.Id, team.Size, leaderResult.Succeeded))
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

        if (dto.Size is not (10 or 25))
        {
            ModelState.AddModelError(nameof(dto.Size), "Size must be set to 10 or 25.");
        }

        var team = new RaidTeam(idGenerator.CreateId())
        {
            Name = dto.Name,
            TeamSize = dto.Size,
            Inactive = dto.Inactive
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
            Inactive = team.Inactive,
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

        if (dto.Size is not (10 or 25))
        {
            ModelState.AddModelError(nameof(dto.Size), "Size must be set to 10 or 25.");
        }

        var team = await _context.RaidTeams.FindAsync(id);

        if (team is null)
        {
            return NotFound();
        }

        team.Name = dto.Name;
        team.Inactive = dto.Inactive;
        team.TeamSize = dto.Size;

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
            Inactive = team.Inactive,
            Schedules = team.Schedules.Select(s => new ScheduleDto
            {
                Day = s.Day,
                RealmTimeStart = s.RealmTimeStart,
                Duration = s.Duration
            }).ToList()
        });
    }

    [HttpPut("{id:long}/members/{characterId:long}"), Authorize(AppPolicies.Recruiter)]
    public async Task<IActionResult> PutMember(long id, long characterId, [FromBody] UpdateTeamMemberDto dto)
    {
        if (dto.MemberStatus < RaidMemberStatus.Member || dto.MemberStatus > RaidMemberStatus.FullTrial)
        {
            ModelState.AddModelError(nameof(dto.MemberStatus), "Unknown member status.");
            return ValidationProblem();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, id, AppPolicies.Recruiter);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var member = await _context.TeamMembers.FindAsync(id, characterId);

        if (member is null)
        {
            return NotFound();
        }

        member.MemberStatus = dto.MemberStatus;

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("TeamMemberStatusUpdated", User, props =>
        {
            props["TeamId"] = id.ToString();
            props["CharacterId"] = characterId.ToString();
        });

        return Accepted();
    }

    [HttpPut("{id:long}/members/{characterId:long}/enchanted")]
    public async Task<IActionResult> PutMemberEnchanted(long id, long characterId, [FromBody] UpdateEnchantedDto dto, [FromServices] MessageSender messageSender)
    {
        var auth = await _authorizationService.AuthorizeAsync(User, id, AppPolicies.LeadershipOrAdmin);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var member = await _context.TeamMembers.FindAsync(id, characterId);

        if (member is null)
        {
            return NotFound();
        }

        if (member.Enchanted == dto.Enchanted)
        {
            return Problem(dto.Enchanted ? "Character is already marked as enchanted." : "Character's enchanted status is already removed.");
        }

        member.Enchanted = dto.Enchanted;

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("TeamMemberEnchantedUpdated", User, props =>
        {
            props["TeamId"] = id.ToString();
            props["CharacterId"] = characterId.ToString();
            props["Enchanted"] = dto.Enchanted.ToString();
        });

        await messageSender.SendGemEnchantMessagesAsync(id, characterId, dto.Enchanted, dto.Message);

        return Accepted();
    }

    [HttpPut("{id:long}/members/{characterId:long}/prepared")]
    public async Task<IActionResult> PutMemberPrepared(long id, long characterId, [FromBody] UpdatePreparedDto dto, [FromServices] MessageSender messageSender)
    {
        var auth = await _authorizationService.AuthorizeAsync(User, id, AppPolicies.LeadershipOrAdmin);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var member = await _context.TeamMembers.FindAsync(id, characterId);

        if (member is null)
        {
            return NotFound();
        }

        if (member.Prepared == dto.Prepared)
        {
            return Problem(dto.Prepared ? "Character is already marked as prepared." : "Character's prepared status is already removed.");
        }

        member.Prepared = dto.Prepared;

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("TeamMemberPreparedUpdated", User, props =>
        {
            props["TeamId"] = member.TeamId.ToString();
            props["CharacterId"] = member.CharacterId.ToString();
            props["Prepared"] = dto.Prepared.ToString();
        });

        await messageSender.SendPreparedMessagesAsync(id, characterId, dto.Prepared, dto.Message);

        return Accepted();
    }

    [HttpPut("{id:long}/members/{characterId:long}/disenchanter")]
    public async Task<IActionResult> PutMemberDisenchanter(long id, long characterId, bool disenchanter)
    {
        var auth = await _authorizationService.AuthorizeAsync(User, id, AppPolicies.LootMasterOrAdmin);

        if (!auth.Succeeded)
        {
            return Unauthorized();
        }

        var member = await _context.TeamMembers.FindAsync(id, characterId);

        if (member is null)
        {
            return NotFound();
        }

        if (member.Disenchanter == disenchanter)
        {
            return Problem(disenchanter ? "Character is already marked as a disenchanter." : "Character is already not a disenchanter.");
        }

        member.Disenchanter = disenchanter;

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("TeamMemberPreparedUpdated", User, props =>
        {
            props["TeamId"] = member.TeamId.ToString();
            props["CharacterId"] = member.CharacterId.ToString();
            props["Disenchanter"] = disenchanter.ToString();
        });

        return Accepted();
    }

    [HttpDelete("{id:long}/members/{characterId:long}")]
    public async Task<IActionResult> DeleteMember(long id, long characterId, [FromServices] IdGen.IIdGenerator<long> idGenerator, [FromServices] DiscordClientProvider dcp)
    {
        var member = await _context.TeamMembers.FindAsync(id, characterId);

        if (member is null)
        {
            return NotFound();
        }

        var auth = await _authorizationService.AuthorizeAsync(User, member.TeamId, AppPolicies.RaidLeader);

        if (!auth.Succeeded)
        {
            auth = await _authorizationService.AuthorizeAsync(User, member.CharacterId, AppPolicies.CharacterOwner);
            if (!auth.Succeeded)
            {
                return Unauthorized();
            }
        }

        var removal = new TeamRemoval(idGenerator.CreateId())
        {
            CharacterId = member.CharacterId,
            RemovedAt = _serverTimeZone.TimeZoneNow(),
            JoinedAt = member.JoinedAt,
            TeamId = member.TeamId
        };

        await foreach (var attendance in _context.RaidAttendees.AsTracking().Where(a => a.CharacterId == member.CharacterId && a.Raid.RaidTeamId == id && a.RemovalId == null).AsAsyncEnumerable())
        {
            attendance.Removal = removal;
            attendance.RemovalId = removal.Id;
        }

        _context.TeamMembers.Remove(member);
        _context.TeamRemovals.Add(removal);

        var extraInfo = await _context.TeamMembers
            .Where(m => m.TeamId == member.TeamId && m.CharacterId == member.CharacterId)
            .Select(m => new { TeamName = m.Team!.Name, m.Team.TeamSize, CharacterName = m.Character!.Name, m.Character.OwnerId })
            .FirstAsync();

        await foreach (var lootList in _context.CharacterLootLists.AsTracking().Where(ll => ll.CharacterId == member.CharacterId && ll.Status != LootListStatus.Locked && ll.Size == extraInfo.TeamSize).AsAsyncEnumerable())
        {
            lootList.Status = LootListStatus.Editing;
        }

        await _context.SaveChangesAsync();

        if (extraInfo.OwnerId > 0)
        {
            await dcp.RemoveRoleAsync(extraInfo.OwnerId.Value, extraInfo.TeamName, "Removed from the raid team.");
        }

        _telemetry.TrackEvent("TeamMemberRemoved", User, props =>
        {
            props["TeamId"] = member.TeamId.ToString();
            props["TeamName"] = extraInfo.TeamName;
            props["CharacterId"] = member.CharacterId.ToString();
            props["CharacterName"] = extraInfo.CharacterName;
            props["RemovalId"] = removal.Id.ToString();
        });

        return Accepted();
    }

    [HttpGet("{id:long}/leaders"), Authorize]
    public async IAsyncEnumerable<GuildMemberDto> GetLeaders(long id, [FromServices] DiscordClientProvider dcp)
    {
        await foreach (var userId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.RaidTeamId == id)
            .Select(rtl => rtl.UserId)
            .AsAsyncEnumerable())
        {
            var member = await dcp.GetMemberAsync(userId);

            if (member is not null)
            {
                yield return dcp.CreateDto(member);
            }
        }
    }

    [HttpPost("{id:long}/leaders"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<GuildMemberDto>> PostLeader(long id, [FromBody] AddLeaderDto dto, [FromServices] DiscordClientProvider dcp, [FromServices] UserManager<AppUser> userManager)
    {
        var team = await _context.RaidTeams.FindAsync(id);

        if (team is null)
        {
            return NotFound();
        }

        var leader = await dcp.GetMemberAsync(dto.MemberId);

        if (leader is null)
        {
            return Problem("Couldn't locate discord user.");
        }

        if (await _context.RaidTeamLeaders.AsNoTracking().CountAsync(rtl => rtl.UserId == dto.MemberId && rtl.RaidTeamId == id) > 0)
        {
            return Problem("Member is already a leader of this team.");
        }

        var userIdString = dto.MemberId.ToString();
        var user = await userManager.FindByIdAsync(userIdString);

        if (user is null)
        {
            user = new AppUser
            {
                Id = dto.MemberId,
                UserName = leader.DisplayName
            };

            var identityResult = await userManager.CreateAsync(user);

            if (!identityResult.Succeeded)
            {
                return Problem($"User creation failed: {identityResult}");
            }

            identityResult = await userManager.AddLoginAsync(user, new UserLoginInfo("Discord", userIdString, "Discord"));

            if (!identityResult.Succeeded)
            {
                return Problem($"User creation failed: {identityResult}");
            }
        }

        _context.RaidTeamLeaders.Add(new() { RaidTeamId = team.Id, UserId = user.Id });
        await _context.SaveChangesAsync();

        await dcp.AssignLeadershipRolesAsync(
            id: dto.MemberId,
            raidLeader: dto.RaidLeader,
            lootMaster: dto.LootMaster,
            recruiter: dto.Recruiter,
            reason: $"Member was assigned as a leader of team {team.Name}.");

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
    public async Task<IActionResult> DeleteLeader(long id, long userId, [FromServices] DiscordClientProvider dcp)
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

        var leaderships = await _context.RaidTeamLeaders
            .AsTracking()
            .Where(rtl => rtl.UserId == userId)
            .ToListAsync();

        var thisLeadership = leaderships.Find(rtl => rtl.RaidTeamId == id);

        if (thisLeadership is null)
        {
            return NotFound();
        }

        _context.RaidTeamLeaders.Remove(thisLeadership);
        await _context.SaveChangesAsync();

        // only unassign leadership roles if this was their only team as a leader.
        if (leaderships.Count == 1)
        {
            await dcp.RemoveLeadershipRolesAsync(userId, $"Member was unassigned as a leader for team {team.Name}.");
        }

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
