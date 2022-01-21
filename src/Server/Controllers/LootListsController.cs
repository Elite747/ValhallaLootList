// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers;

public class LootListsController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;
    private readonly TimeZoneInfo _serverTimeZoneInfo;
    private readonly IAuthorizationService _authorizationService;
    private readonly TelemetryClient _telemetry;
    private readonly DiscordClientProvider _discordClientProvider;

    public LootListsController(
        ApplicationDbContext context,
        TimeZoneInfo serverTimeZoneInfo,
        IAuthorizationService authorizationService,
        TelemetryClient telemetry,
        DiscordClientProvider discordClientProvider)
    {
        _context = context;
        _serverTimeZoneInfo = serverTimeZoneInfo;
        _authorizationService = authorizationService;
        _telemetry = telemetry;
        _discordClientProvider = discordClientProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IList<LootListDto>>> Get(long? characterId = null, long? teamId = null, byte? phase = null, bool? includeApplicants = null)
    {
        try
        {
            var lootLists = await CreateDtosAsync(characterId, teamId, phase, includeApplicants);

            if (lootLists is null)
            {
                return NotFound();
            }

            return Ok(lootLists);
        }
        catch (ArgumentException ex)
        {
            return Problem(ex.Message, statusCode: 400);
        }
    }

    [HttpPost("Phase{phase:int}/{characterId:long}")]
    public async Task<ActionResult<LootListDto>> Post(long characterId, byte phase, [FromBody] LootListSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
    {
        var bracketTemplates = await _context.Brackets.AsNoTracking().Where(b => b.Phase == phase).OrderBy(b => b.Index).ToListAsync();

        if (bracketTemplates.Count == 0)
        {
            return NotFound();
        }

        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, character, AppPolicies.CharacterOwnerOrAdmin);
        if (!authorizationResult.Succeeded)
        {
            return Unauthorized();
        }

        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == phase))
        {
            return Problem("A loot list for that character and phase already exists.");
        }

        if (!dto.MainSpec.IsClass(character.Class))
        {
            ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
            return ValidationProblem();
        }

        if (dto.OffSpec != default && !dto.OffSpec.IsClass(character.Class))
        {
            ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
            return ValidationProblem();
        }

        var list = new CharacterLootList
        {
            CharacterId = character.Id,
            Character = character,
            Entries = new List<LootListEntry>(28),
            Status = LootListStatus.Editing,
            MainSpec = dto.MainSpec,
            OffSpec = dto.OffSpec,
            Phase = phase
        };

        var entries = new List<LootListEntry>();

        var brackets = await _context.Brackets
            .AsNoTracking()
            .Where(b => b.Phase == phase)
            .OrderBy(b => b.Index)
            .ToListAsync();

        foreach (var bracket in brackets)
        {
            for (byte rank = bracket.MinRank; rank <= bracket.MaxRank; rank++)
            {
                for (int col = 0; col < bracket.MaxItems; col++)
                {
                    var entry = new LootListEntry(idGenerator.CreateId())
                    {
                        LootList = list,
                        Rank = rank
                    };
                    _context.LootListEntries.Add(entry);
                    entries.Add(entry);
                }
            }
        }

        _context.CharacterLootLists.Add(list);

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("LootListCreated", User, props =>
        {
            props["CharacterId"] = character.Id.ToString();
            props["CharacterName"] = character.Name;
            props["Phase"] = list.Phase.ToString();
        });

        var scope = await _context.GetCurrentPriorityScopeAsync();
        var now = _serverTimeZoneInfo.TimeZoneNow();
        var donationMatrix = await _context.GetDonationMatrixAsync(d => d.CharacterId == characterId, scope);
        var attendance = await _context.GetAttendanceForCharacterAsync(character, scope.ObservedAttendances);
        var donations = donationMatrix.GetCreditForMonth(characterId, now);

        var returnDto = new LootListDto
        {
            ApprovedBy = null,
            Bonuses = PrioCalculator.GetListBonuses(scope, attendance, character.MemberStatus, donations, character.Enchanted, character.Prepared).ToList(),
            CharacterId = character.Id,
            CharacterMemberStatus = character.MemberStatus,
            CharacterName = character.Name,
            MainSpec = list.MainSpec,
            OffSpec = list.OffSpec,
            Phase = list.Phase,
            Status = list.Status,
            TeamId = character.TeamId,
            Timestamp = list.Timestamp
        };

        if (returnDto.TeamId.HasValue)
        {
            returnDto.TeamName = await _context.RaidTeams.AsNoTracking().Where(t => t.Id == returnDto.TeamId).Select(t => t.Name).FirstOrDefaultAsync();
        }

        foreach (var entry in entries)
        {
            var bracket = brackets.Find(b => entry.Rank >= b.MinRank && entry.Rank <= b.MaxRank);
            Debug.Assert(bracket is not null);
            returnDto.Entries.Add(new LootListEntryDto
            {
                Bracket = bracket.Index,
                BracketAllowsOffspec = bracket.AllowOffspec,
                BracketAllowsTypeDuplicates = bracket.AllowTypeDuplicates,
                Id = entry.Id,
                Rank = entry.Rank,
                AutoPass = entry.AutoPass,
            });
        }

        return CreatedAtAction(nameof(Get), new { characterId = character.Id, phase }, returnDto);
    }

    [HttpPut("Phase{phase:int}/{characterId:long}")]
    public async Task<ActionResult> Put(long characterId, byte phase, [FromBody] LootListSubmissionDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);
        if (character is null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, character, AppPolicies.CharacterOwnerOrAdmin);
        if (!authorizationResult.Succeeded)
        {
            return Unauthorized();
        }

        var list = await _context.CharacterLootLists.FindAsync(characterId, phase);
        if (list is null)
        {
            return NotFound();
        }

        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        if (list.Status != LootListStatus.Editing)
        {
            return Problem("Loot List cannot be edited.");
        }

        if (!dto.MainSpec.IsClass(character.Class))
        {
            ModelState.AddModelError(nameof(dto.MainSpec), "Selected specialization does not fit the player's class.");
            return ValidationProblem();
        }

        if (dto.OffSpec != default && !dto.OffSpec.IsClass(character.Class))
        {
            ModelState.AddModelError(nameof(dto.OffSpec), "Selected specialization does not fit the player's class.");
            return ValidationProblem();
        }

        list.MainSpec = dto.MainSpec;
        list.OffSpec = dto.OffSpec;

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("{characterId:long}/SubmitAll")]
    public async Task<ActionResult<MultiTimestampDto>> SubmitAll(long characterId, [FromBody] SubmitAllListsDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }
        if (!await AuthorizeCharacterAsync(character, AppPolicies.CharacterOwnerOrAdmin))
        {
            return Unauthorized();
        }
        if (character.TeamId.HasValue)
        {
            return Problem("This action is only available for characters not on a raid team.");
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (dto.SubmitTo.Count == 0)
        {
            ModelState.AddModelError(nameof(dto.SubmitTo), "Loot List must be submitted to at least one raid team.");
            return ValidationProblem();
        }

        var submittedPhases = dto.Timestamps.Keys.ToHashSet();
        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase))
            .ToListAsync();

        if (lootLists.Count != currentPhases.Count)
        {
            return Problem("Character does not have a loot list for all current phases.");
        }
        if (!submittedPhases.SetEquals(currentPhases))
        {
            return Problem("Request is missing one or more loot list timestamps.");
        }
        if (!ValidateAllTimestamps(lootLists, dto.Timestamps, out byte invalidPhase))
        {
            return Problem($"The loot list for phase {invalidPhase} has been changed. Refresh before trying again.");
        }
        if (await _context.RaidTeams.AsNoTracking().CountAsync(t => dto.SubmitTo.Contains(t.Id)) != dto.SubmitTo.Count)
        {
            return Problem("One or more raid teams specified do not exist.");
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId)
            .ToListAsync();

        foreach (var id in dto.SubmitTo)
        {
            foreach (var phase in currentPhases)
            {
                if (submissions.Find(s => s.TeamId == id && s.LootListPhase == phase) is null)
                {
                    _context.LootListTeamSubmissions.Add(new()
                    {
                        LootListCharacterId = characterId,
                        LootListPhase = phase,
                        TeamId = id
                    });
                }
            }
        }

        foreach (var submission in submissions)
        {
            if (!dto.SubmitTo.Contains(submission.TeamId))
            {
                _context.LootListTeamSubmissions.Remove(submission);
            }
        }

        foreach (var list in lootLists)
        {
            list.ApprovedBy = null;
            if (list.Status != LootListStatus.Locked)
            {
                list.Status = LootListStatus.Submitted;
            }
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(lootLists);

        await foreach (var leader in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => dto.SubmitTo.Contains(rtl.RaidTeamId))
            .Select(rtl => new { rtl.UserId, rtl.RaidTeamId, TeamName = rtl.RaidTeam.Name })
            .AsAsyncEnumerable())
        {
            if (submissions.Find(s => s.TeamId == leader.RaidTeamId) is null) // Don't notify on a resubmit.
            {
                await _discordClientProvider.SendDmAsync(
                    leader.UserId,
                    $"You have a new application to {leader.TeamName} from {character.Name}. ({character.Race.GetDisplayName()} {character.Class.GetDisplayName()})");
            }
        }

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp) };
    }

    [HttpPost("{characterId:long}/RevokeAll")]
    public async Task<ActionResult<MultiTimestampDto>> RevokeAll(long characterId, [FromBody] MultiTimestampDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }
        if (!await AuthorizeCharacterAsync(character, AppPolicies.CharacterOwnerOrAdmin))
        {
            return Unauthorized();
        }
        if (character.TeamId.HasValue)
        {
            return Problem("This action is only available for characters not on a raid team.");
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var submittedPhases = dto.Timestamps.Keys.ToHashSet();
        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase))
            .ToListAsync();

        if (lootLists.Count != currentPhases.Count)
        {
            return Problem("Character does not have a loot list for all current phases.");
        }
        if (!submittedPhases.SetEquals(currentPhases))
        {
            return Problem("Request is missing one or more loot list timestamps.");
        }
        if (!ValidateAllTimestamps(lootLists, dto.Timestamps, out byte invalidPhase))
        {
            return Problem($"The loot list for phase {invalidPhase} has been changed. Refresh before trying again.");
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        foreach (var list in lootLists)
        {
            list.ApprovedBy = null;
            if (list.Status != LootListStatus.Locked)
            {
                list.Status = LootListStatus.Editing;
            }
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(lootLists);

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp) };
    }

    [HttpPost("{characterId:long}/ApproveAll/{teamId:long}")]
    public async Task<ActionResult<ApproveAllListsResponseDto>> ApproveAll(long characterId, long teamId, [FromBody] ApproveOrRejectAllListsDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }

        var team = await _context.RaidTeams.FindAsync(teamId);

        if (team is null)
        {
            return NotFound();
        }
        if (!await AuthorizeTeamAsync(team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (character.TeamId.HasValue)
        {
            return Problem("This action is only available for characters not on a raid team.");
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var submittedPhases = dto.Timestamps.Keys.ToHashSet();
        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase))
            .ToListAsync();

        if (lootLists.Count != currentPhases.Count)
        {
            return Problem("Character does not have a loot list for all current phases.");
        }
        if (!submittedPhases.SetEquals(currentPhases))
        {
            return Problem("Request is missing one or more loot list timestamps.");
        }
        if (!ValidateAllTimestamps(lootLists, dto.Timestamps, out byte invalidPhase))
        {
            return Problem($"The loot list for phase {invalidPhase} has been changed. Refresh before trying again.");
        }

        if (character.OwnerId > 0)
        {
            var existingCharacterName = await _context.Characters
                .AsNoTracking()
                .Where(c => c.OwnerId == character.OwnerId && c.TeamId == team.Id)
                .Select(c => c.Name)
                .FirstOrDefaultAsync();

            if (existingCharacterName?.Length > 0)
            {
                return Problem($"The owner of this character is already on this team as {existingCharacterName}.");
            }
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId)
            .ToListAsync();

        if (submissions.Find(s => s.TeamId == teamId) is null)
        {
            return Unauthorized();
        }

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        character.TeamId = teamId;
        character.MemberStatus = RaidMemberStatus.FullTrial;
        character.JoinedTeamAt = _serverTimeZoneInfo.TimeZoneNow();

        foreach (var list in lootLists)
        {
            list.ApprovedBy = User.GetDiscordId();
            if (list.Status != LootListStatus.Locked)
            {
                list.Status = LootListStatus.Approved;
            }
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(lootLists);

        await NotifyOwnerAsync(character, team, approved: true, dto.Message);

        return new ApproveAllListsResponseDto
        {
            Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp),
            Member = await TryGetMemberDtoAsync(character, team)
        };
    }

    [HttpPost("{characterId:long}/RejectAll/{teamId:long}")]
    public async Task<ActionResult<MultiTimestampDto>> RejectAll(long characterId, long teamId, [FromBody] ApproveOrRejectAllListsDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }

        var team = await _context.RaidTeams.FindAsync(teamId);

        if (team is null)
        {
            return NotFound();
        }
        if (!await AuthorizeTeamAsync(team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (character.TeamId.HasValue)
        {
            return Problem("This action is only available for characters not on a raid team.");
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase))
            .ToListAsync();

        if (!ValidateAllTimestamps(lootLists, dto.Timestamps, out byte invalidPhase))
        {
            return Problem($"The loot list for phase {invalidPhase} has been changed. Refresh before trying again.");
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId)
            .ToListAsync();

        if (submissions.Find(s => s.TeamId == teamId) is null)
        {
            return Unauthorized();
        }

        _context.LootListTeamSubmissions.RemoveRange(submissions.Where(s => s.TeamId == teamId));

        foreach (var list in lootLists)
        {
            if (!submissions.Any(s => s.TeamId != teamId && s.LootListPhase == list.Phase))
            {
                if (list.Status != LootListStatus.Locked)
                {
                    list.Status = LootListStatus.Editing;
                }

                list.ApprovedBy = null;
            }
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(lootLists);

        await NotifyOwnerAsync(character, team, approved: false, dto.Message);

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp) };
    }

    [HttpPost("Phase{phase:int}/{characterId:long}/Submit")]
    public async Task<ActionResult<TimestampDto>> Submit(long characterId, byte phase, [FromBody] TimestampDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .ThenInclude(c => c.Team)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (!await AuthorizeCharacterAsync(list.Character, AppPolicies.CharacterOwnerOrAdmin))
        {
            return Unauthorized();
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (list.Character.Team is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }

        long teamId = list.Character.Team.Id;
        list.ApprovedBy = null;
        switch (list.Status)
        {
            case LootListStatus.Editing:
                list.Status = LootListStatus.Submitted;
                break;
            case LootListStatus.Submitted:
                return Problem("The loot list has already been submitted.");
            case LootListStatus.Approved:
                return Problem("The loot list has already been approved.");
            case LootListStatus.Locked:
                break;
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == phase)
            .ToListAsync();

        if (submissions.Find(s => s.TeamId == teamId) is null)
        {
            _context.LootListTeamSubmissions.Add(new()
            {
                LootListCharacterId = characterId,
                LootListPhase = phase,
                TeamId = teamId
            });
            _context.LootListTeamSubmissions.RemoveRange(submissions);
        }
        else if (submissions.Count > 1)
        {
            _context.LootListTeamSubmissions.RemoveRange(submissions.Where(s => s.TeamId != teamId));
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        var dm = $"{list.Character.Name} ({list.Character.Race.GetDisplayName()} {list.MainSpec.GetDisplayName(includeClassName: true)}) has submitted a new phase {phase} loot list for team {list.Character.Team.Name}.";

        await foreach (var leaderId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.RaidTeamId == teamId)
            .Select(rtl => rtl.UserId)
            .AsAsyncEnumerable())
        {
            await _discordClientProvider.SendDmAsync(leaderId, dm);
        }

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("Phase{phase:int}/{characterId:long}/Revoke")]
    public async Task<ActionResult<TimestampDto>> Revoke(long characterId, byte phase, [FromBody] TimestampDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .ThenInclude(c => c.Team)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (!await AuthorizeCharacterAsync(list.Character, AppPolicies.CharacterOwnerOrAdmin))
        {
            if (list.Character.Team is null || !await AuthorizeTeamAsync(list.Character.Team, AppPolicies.RaidLeaderOrAdmin))
            {
                return Unauthorized();
            }
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (list.Character.Team is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }

        list.ApprovedBy = null;
        switch (list.Status)
        {
            case LootListStatus.Editing:
                return Problem("The loot list has not been submitted yet.");
            case LootListStatus.Submitted:
            case LootListStatus.Approved:
                list.Status = LootListStatus.Editing;
                break;
            case LootListStatus.Locked:
                return Problem("The loot list is locked.");
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == phase)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("Phase{phase:int}/{characterId:long}/Approve")]
    public async Task<ActionResult<TimestampDto>> Approve(long characterId, byte phase, [FromBody] ApproveOrRejectLootListDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .ThenInclude(c => c.Team)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (list.Character.Team is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!await AuthorizeTeamAsync(list.Character.Team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }

        switch (list.Status)
        {
            case LootListStatus.Editing:
                return Problem("The loot list has not been submitted yet.");
            case LootListStatus.Submitted:
                list.Status = LootListStatus.Approved;
                break;
            case LootListStatus.Approved:
                return Problem("The loot list has already been approved.");
            case LootListStatus.Locked:
                if (list.ApprovedBy.HasValue)
                {
                    return Problem("The loot list is locked.");
                }
                break;
        }
        list.ApprovedBy = User.GetDiscordId();

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == phase)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        await NotifyOwnerAsync(list.Character, list.Character.Team, approved: true, dto.Message);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("Phase{phase:int}/{characterId:long}/Reject")]
    public async Task<ActionResult<TimestampDto>> Reject(long characterId, byte phase, [FromBody] ApproveOrRejectLootListDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .ThenInclude(c => c.Team)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (list.Character.Team is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!await AuthorizeTeamAsync(list.Character.Team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }

        switch (list.Status)
        {
            case LootListStatus.Editing:
                return Problem("The loot list has not been submitted yet.");
            case LootListStatus.Submitted:
                list.Status = LootListStatus.Editing;
                break;
            case LootListStatus.Approved:
                return Problem("The loot list has already been approved.");
            case LootListStatus.Locked:
                if (!list.ApprovedBy.HasValue)
                {
                    return Problem("The loot list is locked.");
                }
                break;
        }
        list.ApprovedBy = null;

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == phase)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        await NotifyOwnerAsync(list.Character, list.Character.Team, approved: false, dto.Message);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("Phase{phase:int}/{characterId:long}/Lock")]
    public async Task<ActionResult<TimestampDto>> Lock(long characterId, byte phase, [FromBody] TimestampDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .ThenInclude(c => c.Team)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (list.Character.Team is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!await AuthorizeTeamAsync(list.Character.Team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }
        if (list.Status != LootListStatus.Approved)
        {
            return Problem("Loot list must be approved before locking.");
        }
        if (!await _context.PhaseActiveAsync(list.Phase))
        {
            return Problem("Phase is not yet active.");
        }

        list.Status = LootListStatus.Locked;

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [Authorize(AppPolicies.Administrator)]
    [HttpPost("Phase{phase:int}/{characterId:long}/Unlock")]
    public async Task<ActionResult<TimestampDto>> Unlock(long characterId, byte phase, [FromBody] TimestampDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == phase)
            .Include(ll => ll.Character)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        if (list.Character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }
        if (list.Status != LootListStatus.Locked)
        {
            return Problem("Loot list is already unlocked.");
        }

        list.Status = LootListStatus.Approved;

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        //await dcp.SendAsync(635355896020729866, m =>
        //{
        //    var userId = (ulong)User.GetDiscordId().GetValueOrDefault();

        //    var request = Url.ActionContext.HttpContext.Request;

        //    var link = request.Scheme + "://" + request.Host + Url.Content($"~/characters/{cname}/phase/{list.Phase}");
        //    m.WithContent($"<@!{userId}> has just unlocked [{cname}'s Phase {list.Phase} Loot List]({link}).")
        //        .WithAllowedMention(new UserMention(userId));
        //});

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    private async Task<MemberDto?> TryGetMemberDtoAsync(Character character, RaidTeam team)
    {
        var scope = await _context.GetCurrentPriorityScopeAsync();
        var members = await HelperQueries.GetMembersAsync(
            _context,
            _serverTimeZoneInfo,
            _context.Characters.AsNoTracking().Where(c => c.Id == character.Id),
            scope,
            team.Id,
            team.Name,
            isLeader: true);
        return members.Count == 1 ? members[0] : null;
    }

    private async Task NotifyOwnerAsync(Character character, RaidTeam team, bool approved, string? message)
    {
        if (character.OwnerId > 0)
        {
            var sb = new StringBuilder("Your application to ")
                .Append(team.Name)
                .Append(" for ")
                .Append(character.Name)
                .Append(" was ")
                .Append(approved ? "approved!" : "rejected.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine().Append("<@").Append(User.GetDiscordId()).Append("> said:");

                foreach (var line in message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine().Append("> ").Append(line);
                }
            }

            if (approved)
            {
                await _discordClientProvider.AddRoleAsync(character.OwnerId.Value, team.Name, "Accepted onto the raid team.");
            }

            await _discordClientProvider.SendDmAsync(character.OwnerId.Value, sb.ToString());
        }
    }

    private void TrackListStatusChange(List<CharacterLootList> lists, [CallerMemberName] string method = null!)
    {
        foreach (var list in lists)
        {
            TrackListStatusChange(list, method);
        }
    }

    private void TrackListStatusChange(CharacterLootList list, [CallerMemberName] string method = null!)
    {
        TrackListStatusChange(list.CharacterId.ToString(), list.Phase.ToString(), list.Status.ToString(), method);
    }

    private void TrackListStatusChange(string characterId, string phase, string status, string method)
    {
        _telemetry.TrackEvent("LootListStatusChanged", User, props =>
        {
            props["CharacterId"] = characterId;
            props["Phase"] = phase;
            props["Status"] = status;
            props["Method"] = method;
        });
    }

    private async Task<List<byte>> GetCurrentPhasesAsync()
    {
        var now = DateTimeOffset.UtcNow;
        return await _context.PhaseDetails
            .AsNoTracking()
            .OrderByDescending(p => p.Id)
            .Where(p => p.StartsAt <= now)
            .Select(p => p.Id)
            .ToListAsync();
    }

    private async Task<bool> AuthorizeCharacterAsync(Character character, string policy)
    {
        var auth = await _authorizationService.AuthorizeAsync(User, character, policy);
        return auth.Succeeded;
    }

    private async Task<bool> AuthorizeTeamAsync(RaidTeam team, string policy)
    {
        var auth = await _authorizationService.AuthorizeAsync(User, team, policy);
        return auth.Succeeded;
    }

    private static bool ValidateTimestamp(CharacterLootList list, byte[] timestamp)
    {
        if (list.Timestamp.Length != timestamp.Length)
        {
            return false;
        }

        for (int i = 0; i < timestamp.Length; i++)
        {
            if (list.Timestamp[i] != timestamp[i])
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValidateAllTimestamps(IEnumerable<CharacterLootList> lootLists, Dictionary<byte, byte[]> timestamps, out byte invalidPhase)
    {
        foreach (var list in lootLists)
        {
            if (timestamps.TryGetValue(list.Phase, out var timestamp) && !ValidateTimestamp(list, timestamp))
            {
                invalidPhase = list.Phase;
                return false;
            }
        }

        invalidPhase = default;
        return true;
    }

    private async Task<IList<LootListDto>?> CreateDtosAsync(long? characterId, long? teamId, byte? phase, bool? includeApplicants)
    {
        var lootListQuery = _context.CharacterLootLists.AsNoTracking();
        var passQuery = _context.DropPasses.AsNoTracking().Where(pass => !pass.WonEntryId.HasValue && pass.RemovalId == null);
        var entryQuery = _context.LootListEntries.AsNoTracking();

        var now = _serverTimeZoneInfo.TimeZoneNow();
        var lastMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0).AddMonths(-1);

        Expression<Func<Donation, bool>> donationPredicate;

        if (characterId.HasValue)
        {
            if (teamId.HasValue)
            {
                throw new ArgumentException("Either characterId or teamId must be set, but not both.");
            }

            var character = await _context.Characters.AsNoTracking().Where(c => c.Id == characterId).Select(c => new { c.Id, c.TeamId }).FirstOrDefaultAsync();

            if (character is null)
            {
                return null;
            }

            lootListQuery = lootListQuery.Where(ll => ll.CharacterId == characterId);
            passQuery = passQuery.Where(p => p.CharacterId == characterId);
            entryQuery = entryQuery.Where(e => e.LootList.CharacterId == characterId);
            donationPredicate = d => d.CharacterId == characterId;
            teamId = character.TeamId;
        }
        else if (teamId.HasValue)
        {
            if (await _context.RaidTeams.CountAsync(team => team.Id == teamId) == 0)
            {
                return null;
            }

            if (includeApplicants == true)
            {
                lootListQuery = lootListQuery.Where(ll => ll.Character.TeamId == teamId || ll.Submissions.Any(s => s.TeamId == teamId));
                entryQuery = entryQuery.Where(e => e.LootList.Character.TeamId == teamId || e.LootList.Submissions.Any(s => s.TeamId == teamId));
            }
            else
            {
                lootListQuery = lootListQuery.Where(ll => ll.Character.TeamId == teamId);
                entryQuery = entryQuery.Where(e => e.LootList.Character.TeamId == teamId);
            }

            passQuery = passQuery.Where(p => p.Character.TeamId == teamId);
            donationPredicate = d => d.Character.TeamId == teamId;
        }
        else
        {
            throw new ArgumentException("Either characterId or teamId must be set.");
        }

        if (phase.HasValue)
        {
            lootListQuery = lootListQuery.Where(ll => ll.Phase == phase.Value);
            entryQuery = entryQuery.Where(e => e.LootList.Phase == phase.Value);
        }

        var userId = User.GetDiscordId();

        var dtos = await lootListQuery
            .Select(ll => new
            {
                ll.ApprovedBy,
                ll.CharacterId,
                CharacterName = ll.Character.Name,
                CharacterMemberStatus = ll.Character.MemberStatus,
                CharacterEnchanted = ll.Character.Enchanted,
                CharacterPrepared = ll.Character.Prepared,
                ll.Character.TeamId,
                TeamName = ll.Character.Team!.Name,
                ll.Status,
                ll.MainSpec,
                ll.OffSpec,
                ll.Phase,
                ll.Timestamp,
                SubmittedTo = ll.Submissions.Select(s => s.TeamId).ToList(),
                Entries = new List<LootListEntryDto>(),
                Bonuses = new List<PriorityBonusDto>(),
                Owned = ll.Character.OwnerId.HasValue && ll.Character.OwnerId == userId,
            })
            .AsSingleQuery()
            .ToListAsync();

        var passes = await passQuery
            .Select(pass => new { pass.CharacterId, pass.RelativePriority, pass.LootListEntryId })
            .ToListAsync();

        var scope = await _context.GetCurrentPriorityScopeAsync();
        var attendanceTable = teamId.HasValue ? await _context.GetAttendanceTableAsync(teamId.Value, scope.ObservedAttendances, characterId) : new();
        var donationMatrix = await _context.GetDonationMatrixAsync(donationPredicate, scope);

        foreach (var dto in dtos)
        {
            attendanceTable.TryGetValue(dto.CharacterId, out var attended);
            var characterDonations = donationMatrix.GetCreditForMonth(dto.CharacterId, now);
            dto.Bonuses.AddRange(PrioCalculator.GetListBonuses(scope, attended, dto.CharacterMemberStatus, characterDonations, dto.CharacterEnchanted, dto.CharacterPrepared));
        }

        var brackets = await _context.Brackets
            .AsNoTracking()
            .OrderBy(b => b.Index)
            .ToListAsync();

        var ownedTeams = new HashSet<long>();

        var adminAuthorizationResult = await _authorizationService.AuthorizeAsync(User, AppPolicies.LeadershipOrAdmin);

        if (!adminAuthorizationResult.Succeeded)
        {
            // querying owned teams is only necessary when not an administrator.
            await foreach (var ownedTeamId in _context.Characters
                .AsNoTracking()
                .Where(c => c.OwnerId == userId && c.TeamId.HasValue)
                .Select(c => c.TeamId!.Value)
                .Distinct()
                .AsAsyncEnumerable())
            {
                ownedTeams.Add(ownedTeamId);
            }
        }

        await foreach (var entry in entryQuery
            .Select(e => new
            {
                e.Id,
                e.ItemId,
                e.Item!.RewardFromId,
                ItemName = (string?)e.Item!.Name,
                Won = e.DropId != null,
                e.Rank,
                e.AutoPass,
                e.Justification,
                e.LootList.Phase,
                e.LootList.CharacterId
            })
            .AsAsyncEnumerable())
        {
            var dto = dtos.Find(x => x.CharacterId == entry.CharacterId && x.Phase == entry.Phase);
            if (dto is not null)
            {
                var bonuses = new List<PriorityBonusDto>();

                if (entry.ItemId.HasValue && !entry.Won)
                {
                    var rewardFromId = entry.RewardFromId ?? entry.ItemId.Value;
                    bonuses.AddRange(PrioCalculator.GetItemBonuses(passes.Count(p => p.LootListEntryId == entry.Id)));
                }

                var entryDto = new LootListEntryDto
                {
                    Bonuses = bonuses,
                    Id = entry.Id,
                    ItemId = entry.ItemId,
                    RewardFromId = entry.RewardFromId,
                    ItemName = entry.ItemName,
                    AutoPass = entry.AutoPass,
                    Won = entry.Won
                };

                if (adminAuthorizationResult.Succeeded || // user is leadership/admin OR
                    dto.Owned || // user owns this character OR
                    (dto.Status == LootListStatus.Locked && dto.TeamId.HasValue && ownedTeams.Contains(dto.TeamId.Value))) // user is on this team and the list is locked
                {
                    entryDto.Justification = entry.Justification;
                    entryDto.Rank = entry.Rank;
                    if (brackets.Find(b => b.Phase == dto.Phase && entry.Rank <= b.MaxRank && entry.Rank >= b.MinRank) is { } bracket)
                    {
                        entryDto.Bracket = bracket.Index;
                        entryDto.BracketAllowsOffspec = bracket.AllowOffspec;
                        entryDto.BracketAllowsTypeDuplicates = bracket.AllowTypeDuplicates;
                    }
                }

                dto.Entries.Add(entryDto);
            }
        }

        var typedDtos = new List<LootListDto>(dtos.Count);

        foreach (var data in dtos)
        {
            data.Entries.Sort((l, r) => string.CompareOrdinal(l.ItemName, r.ItemName));
            typedDtos.Add(new LootListDto
            {
                ApprovedBy = data.ApprovedBy,
                Bonuses = data.Bonuses,
                CharacterId = data.CharacterId,
                CharacterMemberStatus = data.CharacterMemberStatus,
                CharacterName = data.CharacterName,
                Entries = data.Entries,
                MainSpec = data.MainSpec,
                OffSpec = data.OffSpec,
                Phase = data.Phase,
                Status = data.Status,
                SubmittedTo = data.SubmittedTo,
                TeamId = data.TeamId,
                TeamName = data.TeamName,
                Timestamp = data.Timestamp
            });
        }

        return typedDtos;
    }
}
