// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Runtime.CompilerServices;
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
    private readonly MessageSender _messageSender;

    public LootListsController(
        ApplicationDbContext context,
        TimeZoneInfo serverTimeZoneInfo,
        IAuthorizationService authorizationService,
        TelemetryClient telemetry,
        DiscordClientProvider discordClientProvider,
        MessageSender messageSender)
    {
        _context = context;
        _serverTimeZoneInfo = serverTimeZoneInfo;
        _authorizationService = authorizationService;
        _telemetry = telemetry;
        _discordClientProvider = discordClientProvider;
        _messageSender = messageSender;
    }

    [HttpGet]
    public async Task<ActionResult<IList<LootListDto>>> Get(long? characterId = null, long? teamId = null, bool? includeApplicants = null)
    {
        try
        {
            List<LootListDto>? lootLists;

            if (characterId.HasValue)
            {
                if (teamId.HasValue)
                {
                    return BadRequest("Either character or team id must be specified, but not both.");
                }

                lootLists = await CreateDtosForCharacterAsync(characterId.Value);
            }
            else if (teamId.HasValue)
            {
                lootLists = await CreateDtosForTeamAsync(teamId.Value, includeApplicants ?? false);
            }
            else
            {
                return BadRequest("Either character or team id must be specified.");
            }

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

    [HttpPost]
    public async Task<ActionResult<LootListDto>> Post([FromBody] LootListSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
    {
        var bracketTemplates = await _context.Brackets.AsNoTracking().Where(b => b.Phase == dto.Phase).OrderBy(b => b.Index).ToListAsync();

        if (bracketTemplates.Count == 0)
        {
            return NotFound();
        }

        var character = await _context.Characters.FindAsync(dto.CharacterId);

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

        if (await _context.CharacterLootLists.AsNoTracking().AnyAsync(ll => ll.CharacterId == character.Id && ll.Phase == dto.Phase && ll.Size == dto.Size))
        {
            return Problem("A loot list for that character, phase, and raid size already exists.");
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
            Phase = dto.Phase,
            Size = dto.Size,
        };

        var entries = new List<LootListEntry>();

        var brackets = await _context.Brackets
            .AsNoTracking()
            .Where(b => b.Phase == dto.Phase)
            .OrderBy(b => b.Index)
            .ToListAsync();

        foreach (var bracket in brackets)
        {
            for (byte rank = bracket.MinRank; rank <= bracket.MaxRank; rank++)
            {
                for (int col = 0; col < bracket.NormalItems; col++)
                {
                    var entry = new LootListEntry(idGenerator.CreateId())
                    {
                        LootList = list,
                        Rank = rank,
                        Heroic = false
                    };
                    _context.LootListEntries.Add(entry);
                    entries.Add(entry);
                }
                for (int col = 0; col < bracket.HeroicItems; col++)
                {
                    var entry = new LootListEntry(idGenerator.CreateId())
                    {
                        LootList = list,
                        Rank = rank,
                        Heroic = true
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

        var returnDto = new LootListDto
        {
            ApprovedBy = null,
            CharacterId = character.Id,
            CharacterName = character.Name,
            CharacterMemberStatus = RaidMemberStatus.FullTrial,
            MainSpec = list.MainSpec,
            OffSpec = list.OffSpec,
            Phase = list.Phase,
            Size = list.Size,
            Status = list.Status,
            Timestamp = list.Timestamp,
            RanksVisible = true
        };

        var member = await _context.TeamMembers.Where(tm => tm.CharacterId == character.Id && tm.Team!.TeamSize == dto.Size).FirstOrDefaultAsync();
        if (member is not null)
        {
            var bonusTable = await _context.GetBonusTableAsync(member.TeamId, _serverTimeZoneInfo.TimeZoneNow(), member.CharacterId);

            if (bonusTable.TryGetValue(member.CharacterId, out var bonuses))
            {
                returnDto.Bonuses = bonuses;

                if (bonuses.OfType<MembershipPriorityBonusDto>().FirstOrDefault() is { } membership)
                {
                    returnDto.CharacterMemberStatus = membership.Status;
                }
            }

            returnDto.TeamId = member.TeamId;
        }

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
                Heroic = entry.Heroic
            });
        }

        return CreatedAtAction(nameof(Get), new { characterId = character.Id, phase = dto.Phase, size = dto.Size }, returnDto);
    }

    [HttpPut]
    public async Task<ActionResult> Put([FromBody] LootListSubmissionDto dto)
    {
        var character = await _context.Characters.FindAsync(dto.CharacterId);
        if (character is null)
        {
            return NotFound();
        }

        var authorizationResult = await _authorizationService.AuthorizeAsync(User, character, AppPolicies.CharacterOwnerOrAdmin);
        if (!authorizationResult.Succeeded)
        {
            return Unauthorized();
        }

        var list = await _context.CharacterLootLists.FindAsync(dto.CharacterId, dto.Phase, dto.Size);
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
        if (await _context.TeamMembers.AnyAsync(tm => tm.CharacterId == characterId && tm.Team!.TeamSize == dto.Size))
        {
            return Problem($"Character is already on a {dto.Size}-man team.");
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
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase) && ll.Size == dto.Size)
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
            .Where(s => s.LootListCharacterId == characterId && s.LootListSize == dto.Size)
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
                        LootListSize = dto.Size,
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

        await _messageSender.SendNewApplicationMessagesAsync(character, dto.SubmitTo);

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = dto.Size };
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
        if (await _context.TeamMembers.AnyAsync(tm => tm.CharacterId == characterId && tm.Team!.TeamSize == dto.Size))
        {
            return Problem($"Character is already on a {dto.Size}-man team.");
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var submittedPhases = dto.Timestamps.Keys.ToHashSet();
        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase) && ll.Size == dto.Size)
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
            .Where(s => s.LootListCharacterId == characterId && s.LootListSize == dto.Size)
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

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = dto.Size };
    }

    [HttpPost("{characterId:long}/ApproveAll/{teamId:long}")]
    public async Task<ActionResult<ApproveAllListsResponseDto>> ApproveAll(long characterId, long teamId, [FromBody] ApproveOrRejectAllListsDto dto)
    {
        var character = await _context.Characters.FindAsync(characterId);

        if (character is null)
        {
            return NotFound();
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var team = await _context.RaidTeams.FindAsync(teamId);

        if (team is null)
        {
            return NotFound();
        }
        if (team.TeamSize != dto.Size)
        {
            return Problem("Requested team size does not match the size of the team.");
        }
        if (!await AuthorizeTeamAsync(team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (await _context.TeamMembers.AnyAsync(tm => tm.CharacterId == characterId && tm.Team!.TeamSize == dto.Size))
        {
            return Problem($"Character is already on a {dto.Size}-man team.");
        }
        if (await _context.TeamMembers.CountAsync(tm => tm.TeamId == teamId) >= (team.TeamSize == 10 ? 13 : 30))
        {
            return Problem($"{team.Name} already has the maximum number of members assigned to it.");
        }

        var submittedPhases = dto.Timestamps.Keys.ToHashSet();
        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase) && ll.Size == dto.Size)
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
            var existingCharacterName = await _context.TeamMembers
                .AsNoTracking()
                .Where(tm => tm.Character!.OwnerId == character.OwnerId && tm.TeamId == team.Id)
                .Select(tm => tm.Character!.Name)
                .FirstOrDefaultAsync();

            if (existingCharacterName?.Length > 0)
            {
                return Problem($"The owner of this character is already on this team as {existingCharacterName}.");
            }
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListSize == dto.Size)
            .ToListAsync();

        if (submissions.Find(s => s.TeamId == teamId) is null)
        {
            return Unauthorized();
        }

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        _context.TeamMembers.Add(new()
        {
            JoinedAt = _serverTimeZoneInfo.TimeZoneNow(),
            TeamId = teamId,
            CharacterId = characterId,
            Disenchanter = false,
            Enchanted = false,
            Prepared = false,
            Bench = dto.Bench
        });

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

        await _messageSender.SendApprovedApplicationMessagesAsync(character, team, dto.Message);

        return new ApproveAllListsResponseDto
        {
            Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp),
            Member = await TryGetMemberDtoAsync(character, team),
            Size = team.TeamSize
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
        if (team.TeamSize != dto.Size)
        {
            return Problem("Requested team size does not match the size of the team.");
        }
        if (!await AuthorizeTeamAsync(team, AppPolicies.RaidLeaderOrAdmin))
        {
            return Unauthorized();
        }
        if (character.Deactivated)
        {
            return Problem("Character has been deactivated.");
        }

        var currentPhases = await GetCurrentPhasesAsync();

        var lootLists = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && currentPhases.Contains(ll.Phase) && ll.Size == dto.Size)
            .ToListAsync();

        if (!ValidateAllTimestamps(lootLists, dto.Timestamps, out byte invalidPhase))
        {
            return Problem($"The loot list for phase {invalidPhase} has been changed. Refresh before trying again.");
        }

        var submissions = await _context.LootListTeamSubmissions
            .AsTracking()
            .Where(s => s.LootListCharacterId == characterId && s.LootListSize == dto.Size)
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

        await _messageSender.SendDeniedApplicationMessagesAsync(character, team, dto.Message);

        return new MultiTimestampDto { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = dto.Size };
    }

    [HttpPost("{characterId:long}/Submit")]
    public async Task<ActionResult<TimestampDto>> Submit(long characterId, [FromBody] LootListActionDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
            .Include(ll => ll.Character)
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
        if (!ValidateTimestamp(list, dto.Timestamp))
        {
            return Problem("The loot list has been changed. Refresh before trying again.");
        }

        var member = await _context.TeamMembers.AsTracking()
            .Where(m => m.Team!.TeamSize == dto.Size && m.CharacterId == characterId)
            .Include(m => m.Team)
            .FirstOrDefaultAsync();

        if (member is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }

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
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == dto.Phase && s.LootListSize == dto.Size)
            .ToListAsync();

        if (submissions.Find(s => s.TeamId == member.TeamId) is null)
        {
            _context.LootListTeamSubmissions.Add(new()
            {
                LootListCharacterId = characterId,
                LootListPhase = dto.Phase,
                LootListSize = dto.Size,
                TeamId = member.TeamId
            });
            _context.LootListTeamSubmissions.RemoveRange(submissions);
        }
        else if (submissions.Count > 1)
        {
            _context.LootListTeamSubmissions.RemoveRange(submissions.Where(s => s.TeamId != member.TeamId));
        }

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        await _messageSender.SendNewListMessagesAsync(list, member.Team!);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("{characterId:long}/Revoke")]
    public async Task<ActionResult<TimestampDto>> Revoke(long characterId, [FromBody] LootListActionDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
            .Include(ll => ll.Character)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }

        var member = await _context.TeamMembers.AsTracking()
            .Where(m => m.Team!.TeamSize == dto.Size && m.CharacterId == characterId)
            .Include(m => m.Team)
            .FirstOrDefaultAsync();

        if (member is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }

        if (!await AuthorizeCharacterAsync(list.Character, AppPolicies.CharacterOwnerOrAdmin) &&
            !await AuthorizeTeamAsync(member.Team!, AppPolicies.RaidLeaderOrAdmin))
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
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == dto.Phase && s.LootListSize == dto.Size)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("{characterId:long}/Approve")]
    public async Task<ActionResult<TimestampDto>> Approve(long characterId, [FromBody] ApproveOrRejectLootListDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
            .Include(ll => ll.Character)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }

        var member = await _context.TeamMembers.AsTracking()
            .Where(m => m.Team!.TeamSize == dto.Size && m.CharacterId == characterId)
            .Include(m => m.Team)
            .FirstOrDefaultAsync();

        if (member is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }

        if (!await AuthorizeTeamAsync(member.Team!, AppPolicies.RaidLeaderOrAdmin))
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
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == dto.Phase && s.LootListSize == dto.Size)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        await _messageSender.SendApprovedListMessagesAsync(list.Character, list, dto.Message);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("{characterId:long}/Reject")]
    public async Task<ActionResult<TimestampDto>> Reject(long characterId, [FromBody] ApproveOrRejectLootListDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
            .Include(ll => ll.Character)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        var member = await _context.TeamMembers.AsTracking()
            .Where(m => m.Team!.TeamSize == dto.Size && m.CharacterId == characterId)
            .Include(m => m.Team)
            .FirstOrDefaultAsync();

        if (member is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!await AuthorizeTeamAsync(member.Team!, AppPolicies.RaidLeaderOrAdmin))
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
            .Where(s => s.LootListCharacterId == characterId && s.LootListPhase == dto.Phase && s.LootListSize == dto.Size)
            .ToListAsync();

        _context.LootListTeamSubmissions.RemoveRange(submissions);

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        await _messageSender.SendDeniedListMessagesAsync(list.Character, list, dto.Message);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [HttpPost("{characterId:long}/Lock")]
    public async Task<ActionResult<TimestampDto>> Lock(long characterId, [FromBody] LootListActionDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
            .Include(ll => ll.Character)
            .SingleOrDefaultAsync();

        if (list is null)
        {
            return NotFound();
        }
        var member = await _context.TeamMembers.AsTracking()
            .Where(m => m.Team!.TeamSize == dto.Size && m.CharacterId == characterId)
            .Include(m => m.Team)
            .FirstOrDefaultAsync();

        if (member is null)
        {
            return Problem("This action is only available for characters on a raid team.");
        }
        if (!await AuthorizeTeamAsync(member.Team!, AppPolicies.RaidLeaderOrAdmin))
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

        list.Status = LootListStatus.Locked;

        await _context.SaveChangesAsync();

        TrackListStatusChange(list);

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    [Authorize(AppPolicies.Administrator)]
    [HttpPost("{characterId:long}/Unlock")]
    public async Task<ActionResult<TimestampDto>> Unlock(long characterId, [FromBody] LootListActionDto dto)
    {
        var list = await _context.CharacterLootLists.AsTracking()
            .Where(ll => ll.CharacterId == characterId && ll.Phase == dto.Phase && ll.Size == dto.Size)
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

        await _discordClientProvider.SendOrUpdateOfficerNotificationAsync(null, m =>
        {
            var userId = (ulong)User.GetDiscordId().GetValueOrDefault();

            m.WithContent($"<@!{userId}> has just unlocked {list.Character.Name}'s Phase {list.Phase} {list.Size}-Man Loot List.")
                .WithAllowedMention(new DSharpPlus.Entities.UserMention(userId));
        });

        return new TimestampDto { Timestamp = list.Timestamp };
    }

    private async Task<MemberDto?> TryGetMemberDtoAsync(Character character, RaidTeam team)
    {
        var members = await HelperQueries.GetMembersAsync(
            _context,
            _serverTimeZoneInfo,
            team.Id,
            team.TeamSize,
            isLeader: true,
            characterId: character.Id);
        return members.Find(m => m.Character.Id == character.Id);
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

    private async Task<List<LootListDto>?> CreateDtosForTeamAsync(long teamId, bool includeApplicants)
    {
        var userId = User.GetDiscordId();

        var team = await _context.RaidTeams.FindAsync(teamId);

        if (team is null)
        {
            return null;
        }

        var leaders = await _context.RaidTeamLeaders.Where(l => l.RaidTeamId == teamId).Select(l => l.UserId).ToListAsync();

        var lootLists = await _context.CharacterLootLists.AsNoTracking()
            .Where(ll => ll.Size == team.TeamSize && (ll.Character.Teams.Any(tm => tm.TeamId == teamId) || (includeApplicants && ll.Submissions.Any(s => s.TeamId == teamId))))
            .Select(ll => new
            {
                ll.ApprovedBy,
                ll.CharacterId,
                CharacterName = ll.Character.Name,
                ll.Status,
                ll.MainSpec,
                ll.OffSpec,
                ll.Phase,
                ll.Size,
                ll.Timestamp,
                SubmittedTo = ll.Submissions.Select(s => s.TeamId).ToList(),
                Entries = new List<LootListEntryDto>(),
                Bonuses = new List<PriorityBonusDto>(),
                ll.Character.OwnerId
            })
            .AsSingleQuery()
            .ToListAsync();

        if (lootLists.Count == 0)
        {
            return new();
        }

        var members = await _context.TeamMembers.AsNoTracking().Where(tm => tm.TeamId == teamId).ToListAsync();

        var passes = await _context.Drops.AsNoTracking()
            .Where(drop => drop.EncounterKill.Raid.RaidTeamId == teamId)
            .Select(drop => new { drop.ItemId, drop.EncounterKill.KilledAt })
            .ToListAsync();

        var bonusTable = await _context.GetBonusTableAsync(teamId, _serverTimeZoneInfo.TimeZoneNow());

        foreach (var dto in lootLists)
        {
            var member = members.Find(m => m.CharacterId == dto.CharacterId);

            if (member is not null && bonusTable.TryGetValue(member.CharacterId, out var bonuses))
            {
                dto.Bonuses.AddRange(bonuses);
            }
        }

        var brackets = await _context.Brackets
            .AsNoTracking()
            .OrderBy(b => b.Index)
            .ToListAsync();

        var userLists = await _context.CharacterLootLists
            .AsNoTracking()
            .Where(ll => ll.Character.OwnerId == userId)
            .Select(ll => new { ll.Status, ll.Phase, ll.Size, ll.CharacterId })
            .ToListAsync();

        var userTeams = await _context.TeamMembers
            .AsNoTracking()
            .Where(m => m.Character!.OwnerId == userId)
            .Select(m => new { m.CharacterId, m.TeamId })
            .ToListAsync();

        bool isAdmin = (await _authorizationService.AuthorizeAsync(User, AppPolicies.Administrator)).Succeeded;

        await foreach (var entry in _context.LootListEntries.AsNoTracking()
            .Where(e => e.LootList.Size == team.TeamSize && (e.LootList.Character.Teams.Any(tm => tm.TeamId == teamId) || (includeApplicants && e.LootList.Submissions.Any(s => s.TeamId == teamId))))
            .Select(e => new
            {
                e.Id,
                e.ItemId,
                e.Item!.RewardFromId,
                ItemName = (string?)e.Item!.Name,
                Won = e.DropId != null,
                e.Rank,
                e.Heroic,
                e.AutoPass,
                Justification = (string?)e.Justification,
                e.LootList.Phase,
                e.LootList.CharacterId
            })
            .AsAsyncEnumerable())
        {
            var list = lootLists.Find(x => x.CharacterId == entry.CharacterId && x.Phase == entry.Phase);
            if (list is not null)
            {
                var bonuses = new List<PriorityBonusDto>();

                if (entry.ItemId.HasValue && !entry.Won && members.Find(m => m.CharacterId == list.CharacterId) is { } member)
                {
                    var rewardFromId = entry.RewardFromId ?? entry.ItemId.Value;
                    bonuses.AddRange(PrioCalculator.GetItemBonuses(passes.Count(p => p.KilledAt >= member.JoinedAt && p.ItemId == rewardFromId)));
                }

                var entryDto = new LootListEntryDto
                {
                    Bonuses = bonuses,
                    Id = entry.Id,
                    ItemId = entry.ItemId,
                    RewardFromId = entry.RewardFromId,
                    ItemName = entry.ItemName,
                    AutoPass = entry.AutoPass,
                    Won = entry.Won,
                    Heroic = entry.Heroic
                };

                if (
                    (list.OwnerId is null && isAdmin) || // user is an admin and the list has no owner
                    (userId.HasValue && list.OwnerId == userId) || // user owns this character OR
                    (
                        (
                            list.Status == LootListStatus.Locked ||
                            (userId.HasValue && leaders.Contains(userId.Value))
                        ) &&
                        userLists.Any(l => l.Size == list.Size && l.Status == LootListStatus.Locked && l.Phase == list.Phase && userTeams.Any(team => team.TeamId == teamId && team.CharacterId == l.CharacterId))
                    ) || // user is on this team and both this and their list is locked, or user is a leader of this team with a locked list
                    (userId.HasValue && list.OwnerId.HasValue && leaders.Contains(userId.Value) && leaders.Contains(list.OwnerId.Value)) // user is a leader of this team and target list is of a leader of this team
                    )
                {
                    entryDto.Justification = entry.Justification;
                    entryDto.Rank = entry.Rank;
                    if (brackets.Find(b => b.Phase == list.Phase && entry.Rank <= b.MaxRank && entry.Rank >= b.MinRank) is { } bracket)
                    {
                        entryDto.Bracket = bracket.Index;
                        entryDto.BracketAllowsOffspec = bracket.AllowOffspec;
                        entryDto.BracketAllowsTypeDuplicates = bracket.AllowTypeDuplicates;
                    }
                }

                list.Entries.Add(entryDto);
            }
        }

        var typedDtos = new List<LootListDto>(lootLists.Count);

        foreach (var list in lootLists)
        {
            list.Entries.Sort((l, r) => string.CompareOrdinal(l.ItemName, r.ItemName));
            typedDtos.Add(new LootListDto
            {
                ApprovedBy = list.ApprovedBy,
                Bonuses = list.Bonuses,
                CharacterId = list.CharacterId,
                CharacterMemberStatus = list.Bonuses.OfType<MembershipPriorityBonusDto>().FirstOrDefault()?.Status ?? RaidMemberStatus.FullTrial,
                CharacterName = list.CharacterName,
                Entries = list.Entries,
                MainSpec = list.MainSpec,
                OffSpec = list.OffSpec,
                Phase = list.Phase,
                Size = list.Size,
                Status = list.Status,
                SubmittedTo = list.SubmittedTo,
                TeamId = members?.Find(m => m.CharacterId == list.CharacterId)?.TeamId,
                TeamName = team.Name,
                Timestamp = list.Timestamp,
                RanksVisible = list.Entries.All(e => e.Rank > 0)
            });
        }

        return typedDtos;
    }

    private async Task<List<LootListDto>?> CreateDtosForCharacterAsync(long characterId)
    {
        var userId = User.GetDiscordId();

        var lootLists = await _context.CharacterLootLists.AsNoTracking()
            .Where(ll => ll.CharacterId == characterId)
            .Select(ll => new
            {
                ll.ApprovedBy,
                ll.CharacterId,
                CharacterName = ll.Character.Name,
                ll.Status,
                ll.MainSpec,
                ll.OffSpec,
                ll.Phase,
                ll.Size,
                ll.Timestamp,
                SubmittedTo = ll.Submissions.Select(s => s.TeamId).ToList(),
                Entries = new List<LootListEntryDto>(),
                Bonuses = new List<PriorityBonusDto>(),
                ll.Character.OwnerId
            })
            .AsSingleQuery()
            .ToListAsync();

        if (lootLists.Count == 0)
        {
            return new();
        }

        var leaders = await _context.RaidTeamLeaders
            .AsNoTracking()
            .Select(rtl => new { rtl.RaidTeamId, rtl.UserId })
            .ToListAsync();

        var members = await _context.TeamMembers.AsNoTracking()
            .Where(tm => tm.CharacterId == characterId)
            .Select(tm => new { tm.Enchanted, tm.Prepared, tm.TeamId, tm.Team!.TeamSize, TeamName = tm.Team.Name, tm.JoinedAt })
            .ToListAsync();

        var passesByTeam = new Dictionary<long, Dictionary<uint, int>>();

        if (members.Count != 0)
        {
            var now = _serverTimeZoneInfo.TimeZoneNow();

            foreach (var member in members)
            {
                var passes = await _context.Drops.AsNoTracking()
                    .Where(drop => drop.EncounterKill.Raid.RaidTeamId == member.TeamId && drop.EncounterKill.KilledAt >= member.JoinedAt && drop.EncounterKill.Characters.Any(cek => cek.CharacterId == characterId))
                    .GroupBy(drop => drop.ItemId)
                    .Select(g => new { ItemId = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(g => g.ItemId, g => g.Count);

                passesByTeam[member.TeamId] = passes;

                var bonusTable = await _context.GetBonusTableAsync(member.TeamId, now, characterId);

                if (bonusTable.TryGetValue(characterId, out var bonuses))
                {
                    foreach (var lootList in lootLists.Where(ll => ll.Size == member.TeamSize))
                    {
                        lootList.Bonuses.AddRange(bonuses);
                    }
                }
            }
        }

        var brackets = await _context.Brackets
            .AsNoTracking()
            .OrderBy(b => b.Index)
            .ToListAsync();

        var userLists = await _context.CharacterLootLists
            .AsNoTracking()
            .Where(ll => ll.Character.OwnerId == userId)
            .Select(ll => new { ll.Status, ll.Phase, ll.Size, ll.CharacterId })
            .ToListAsync();

        var userTeams = await _context.TeamMembers
            .AsNoTracking()
            .Where(m => m.Character!.OwnerId == userId)
            .Select(m => new { m.TeamId, m.CharacterId })
            .ToListAsync();

        bool isAdmin = (await _authorizationService.AuthorizeAsync(User, AppPolicies.Administrator)).Succeeded;

        await foreach (var entry in _context.LootListEntries.AsNoTracking()
            .Where(e => e.LootList.CharacterId == characterId)
            .Select(e => new
            {
                e.Id,
                e.ItemId,
                e.Item!.RewardFromId,
                e.Heroic,
                ItemName = (string?)e.Item!.Name,
                Won = e.DropId != null,
                e.Rank,
                e.AutoPass,
                Justification = (string?)e.Justification,
                e.LootList.Phase,
                e.LootList.CharacterId,
                e.LootList.Size
            })
            .AsAsyncEnumerable())
        {
            var list = lootLists.Find(x => x.Size == entry.Size && x.Phase == entry.Phase);
            if (list is not null)
            {
                var member = members.Find(m => m.TeamSize == list.Size);
                var bonuses = new List<PriorityBonusDto>();

                if (entry.ItemId.HasValue && !entry.Won && member is not null && passesByTeam.TryGetValue(member.TeamId, out var passesByItem))
                {
                    if (entry.RewardFromId is null || !passesByItem.TryGetValue(entry.RewardFromId.Value, out int passes))
                    {
                        passesByItem.TryGetValue(entry.ItemId.Value, out passes);
                    }

                    bonuses.AddRange(PrioCalculator.GetItemBonuses(passes));
                }

                var entryDto = new LootListEntryDto
                {
                    Bonuses = bonuses,
                    Id = entry.Id,
                    ItemId = entry.ItemId,
                    RewardFromId = entry.RewardFromId,
                    ItemName = entry.ItemName,
                    AutoPass = entry.AutoPass,
                    Won = entry.Won,
                    Heroic = entry.Heroic
                };

                if (
                    (list.OwnerId is null && isAdmin) || // user is an admin and the list has no owner
                    (userId.HasValue && list.OwnerId == userId) || // user owns this character OR
                    (member is not null &&
                        (list.Status == LootListStatus.Locked || leaders.Any(l => l.UserId == userId && l.RaidTeamId == member.TeamId)) &&
                        userLists.Any(l => l.Size == member.TeamSize && l.Status == LootListStatus.Locked && l.Phase == list.Phase && userTeams.Any(team => team.TeamId == member.TeamId && team.CharacterId == l.CharacterId))
                        ) || // user is on this team and both this and their list is locked OR user is a leader of this team
                    (userId.HasValue && list.OwnerId.HasValue &&
                        leaders.Any(l => l.UserId == userId.Value && (l.RaidTeamId == member?.TeamId || list.SubmittedTo.Contains(l.RaidTeamId))) &&
                        leaders.Any(l => l.UserId == list.OwnerId.Value && (l.RaidTeamId == member?.TeamId || list.SubmittedTo.Contains(l.RaidTeamId)))
                        ) // user is a leader of this team and target list is of a leader of this team
                    )
                {
                    entryDto.Justification = entry.Justification;
                    entryDto.Rank = entry.Rank;
                    if (brackets.Find(b => b.Phase == list.Phase && entry.Rank <= b.MaxRank && entry.Rank >= b.MinRank) is { } bracket)
                    {
                        entryDto.Bracket = bracket.Index;
                        entryDto.BracketAllowsOffspec = bracket.AllowOffspec;
                        entryDto.BracketAllowsTypeDuplicates = bracket.AllowTypeDuplicates;
                    }
                }

                list.Entries.Add(entryDto);
            }
        }

        var typedDtos = new List<LootListDto>(lootLists.Count);

        foreach (var list in lootLists)
        {
            var member = members.Find(m => m.TeamSize == list.Size);
            list.Entries.Sort((l, r) => string.CompareOrdinal(l.ItemName, r.ItemName));
            typedDtos.Add(new LootListDto
            {
                ApprovedBy = list.ApprovedBy,
                Bonuses = list.Bonuses,
                CharacterId = list.CharacterId,
                CharacterMemberStatus = list.Bonuses.OfType<MembershipPriorityBonusDto>().FirstOrDefault()?.Status ?? RaidMemberStatus.FullTrial,
                CharacterName = list.CharacterName,
                Entries = list.Entries,
                MainSpec = list.MainSpec,
                OffSpec = list.OffSpec,
                Phase = list.Phase,
                Size = list.Size,
                Status = list.Status,
                SubmittedTo = list.SubmittedTo,
                TeamId = member?.TeamId,
                TeamName = member?.TeamName,
                Timestamp = list.Timestamp,
                RanksVisible = list.Entries.All(e => e.Rank > 0)
            });
        }

        return typedDtos;
    }
}
