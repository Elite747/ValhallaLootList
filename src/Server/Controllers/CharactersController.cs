// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers;

public class CharactersController(ApplicationDbContext context, TelemetryClient telemetry) : ApiControllerV1
{
    private readonly ApplicationDbContext _context = context;
    private readonly TelemetryClient _telemetry = telemetry;

    [HttpGet]
    public IAsyncEnumerable<CharacterDto> Get(string? team = null, bool? inclDeactivated = null)
    {
        var query = _context.Characters.AsNoTracking();

        if (team?.Length > 0)
        {
            if (string.Equals(team, "none", StringComparison.InvariantCultureIgnoreCase))
            {
                query = query.Where(c => c.Teams.Count == 0);
            }
            else if (long.TryParse(team, out var teamId))
            {
                query = query.Where(c => c.Teams.Any(tm => tm.TeamId == teamId));
            }
            else
            {
                query = query.Where(c => c.Teams.Any(tm => tm.Team!.Name == team));
            }
        }

        if (inclDeactivated == false)
        {
            query = query.Where(c => !c.Deactivated);
        }

        return query
            .OrderBy(c => c.Name)
            .Select(c => new CharacterDto
            {
                Class = c.Class,
                Id = c.Id,
                Name = c.Name,
                Race = c.Race,
                Deactivated = c.Deactivated,
                Verified = c.VerifiedById.HasValue,
                Teams = c.Teams.Select(tm => tm.TeamId).ToList()
            })
            .AsAsyncEnumerable();
    }

    [HttpGet("{id:long}")]
    public Task<ActionResult<CharacterDto>> Get(long id)
    {
        return GetAsync(c => c.Id == id);
    }

    [HttpGet("ByName/{name}")]
    public Task<ActionResult<CharacterDto>> GetByName(string name)
    {
        return GetAsync(c => c.Name == name);
    }

    [HttpGet("@mine")]
    public IAsyncEnumerable<CharacterDto> GetMine()
    {
        var userId = User.GetDiscordId();
        Debug.Assert(userId.HasValue);
        return _context.Characters
            .AsNoTracking()
            .Where(c => c.OwnerId == userId)
            .OrderBy(c => c.Name)
            .Select(c => new CharacterDto
            {
                Class = c.Class,
                Id = c.Id,
                Name = c.Name,
                Race = c.Race,
                Deactivated = c.Deactivated,
                Verified = c.VerifiedById.HasValue,
                Teams = c.Teams.Select(tm => tm.TeamId).ToList()
            })
            .AsAsyncEnumerable();
    }

    private async Task<ActionResult<CharacterDto>> GetAsync(Expression<Func<Character, bool>> match)
    {
        var character = await _context.Characters
            .AsNoTracking()
            .Where(match)
            .Select(c => new CharacterDto
            {
                Class = c.Class,
                Id = c.Id,
                Name = c.Name,
                Race = c.Race,
                Deactivated = c.Deactivated,
                Verified = c.VerifiedById.HasValue,
                Teams = c.Teams.Select(tm => tm.TeamId).ToList()
            })
            .FirstOrDefaultAsync();

        if (character is null)
        {
            return NotFound();
        }

        return character;
    }

    [HttpPost]
    public async Task<ActionResult<CharacterDto>> Post([FromBody] CharacterSubmissionDto dto, [FromServices] IdGen.IIdGenerator<long> idGenerator)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem();
        }

        Debug.Assert(dto.Name?.Length > 1);
        Debug.Assert(dto.Race.HasValue);

        var normalizedName = NameHelpers.NormalizeName(dto.Name);

        if (await _context.Characters.AsNoTracking().AnyAsync(c => c.Name.Equals(normalizedName)))
        {
            ModelState.AddModelError(
                nameof(dto.Name),
                "A character with that name already exists. If this is your character, message an officer to have it added to your account.");
            return ValidationProblem();
        }

        if (!dto.Race.Value.IsValidRace())
        {
            ModelState.AddModelError(nameof(dto.Race), "Race selection is not valid.");
        }

        if (!dto.Class.IsSingleClass())
        {
            ModelState.AddModelError(nameof(dto.Class), "Class selection is not valid.");
            return ValidationProblem();
        }

        if (!dto.Race.Value.GetClasses().Contains(dto.Class))
        {
            ModelState.AddModelError(nameof(dto.Class), "Class is not available to the selected race.");
            return ValidationProblem();
        }

        var character = new Character(idGenerator.CreateId())
        {
            Class = dto.Class,
            Name = normalizedName,
            Race = dto.Race.Value,
        };

        if (dto.SenderIsOwner)
        {
            character.OwnerId = User.GetDiscordId();

            if (User.IsAdmin())
            {
                character.VerifiedById = character.OwnerId;
            }
        }

        _context.Characters.Add(character);

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterCreated", character);

        return CreatedAtAction(nameof(Get), new { id = character.Id }, new CharacterDto
        {
            Class = character.Class,
            Id = character.Id,
            Name = character.Name,
            Race = character.Race,
            Deactivated = character.Deactivated,
            Verified = character.VerifiedById.HasValue
        });
    }

    [HttpPut("{id:long}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult<CharacterDto>> Put(long id, [FromBody] CharacterSubmissionDto dto)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        if (character.Class != dto.Class)
        {
            ModelState.AddModelError(nameof(dto.Class), "Can't change the class of a character.");
            return ValidationProblem();
        }

        if (dto.Name?.Length > 0 && character.Name != dto.Name)
        {
            var normalizedName = NameHelpers.NormalizeName(dto.Name);

            if (await _context.Characters.AsNoTracking().AnyAsync(c => c.Name.Equals(normalizedName)))
            {
                ModelState.AddModelError(nameof(dto.Name), "A character with that name already exists.");
                return ValidationProblem();
            }

            character.Name = normalizedName;
        }

        if (dto.Race.HasValue)
        {
            character.Race = dto.Race.Value;
        }

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterUpdated", character);

        return new CharacterDto
        {
            Class = character.Class,
            Id = character.Id,
            Name = character.Name,
            Race = character.Race,
            Deactivated = character.Deactivated,
            Verified = character.VerifiedById.HasValue,
            Teams = await _context.TeamMembers.Where(tm => tm.CharacterId == id).Select(tm => tm.TeamId).ToListAsync()
        };
    }

    [HttpGet("{characterId:long}/Donations")]
    public async Task<ActionResult<DonationSummaryDto>> GetDonationSummary(long characterId, [FromServices] TimeZoneInfo serverTimeZoneInfo)
    {
        if (await _context.Characters.FindAsync(characterId) is null)
        {
            return NotFound();
        }

        var thisMonth = serverTimeZoneInfo.TimeZoneNow();
        var nextMonth = thisMonth.AddMonths(1);

        var donations = await _context.Donations
            .AsNoTracking()
            .Where(d => d.CharacterId == characterId && (
                (d.TargetYear == thisMonth.Year && d.TargetMonth == thisMonth.Month) ||
                (d.TargetYear == nextMonth.Year && d.TargetMonth == nextMonth.Month)
            ))
            .GroupBy(d => d.TargetMonth)
            .Select(g => new { Month = (int)g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.Month, g => g.Count);

        var isFulltime = await _context.TeamMembers
            .AsNoTracking()
            .Where(m => m.CharacterId == characterId && m.Team!.TeamSize == 25 && !m.Bench)
            .AnyAsync();

        return new DonationSummaryDto
        {
            ThisMonth = donations.GetValueOrDefault(thisMonth.Month),
            NextMonth = donations.GetValueOrDefault(nextMonth.Month),
            Maximum = isFulltime ? PrioCalculator.MaxDonations : 1
        };
    }

    [HttpGet("{id:long}/Admin"), Authorize(AppPolicies.LeadershipOrAdmin)]
    public async Task<ActionResult<CharacterAdminDto>> GetAdmin(long id, [FromServices] DiscordClientProvider dcp)
    {
        var character = await _context.Characters
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new { c.VerifiedById, c.OwnerId })
            .FirstOrDefaultAsync();

        if (character is null)
        {
            return NotFound();
        }

        var removals = await _context.TeamRemovals
            .AsNoTracking()
            .Where(r => r.CharacterId == id)
            .OrderByDescending(r => r.RemovedAt)
            .Select(r => new TeamRemovalDto
            {
                Id = r.Id,
                RemovedAt = r.RemovedAt,
                TeamId = r.TeamId,
                TeamName = r.Team.Name,
                JoinedAt = r.JoinedAt
            })
            .ToListAsync();

        var otherCharacters = new List<string>();

        if (character.OwnerId.HasValue)
        {
            await foreach (var charName in _context.Characters
                .AsNoTracking()
                .Where(c => c.OwnerId == character.OwnerId && c.Id != id)
                .OrderBy(c => c.Name)
                .Select(c => c.Name)
                .AsAsyncEnumerable())
            {
                otherCharacters.Add(charName);
            }
        }

        return new CharacterAdminDto
        {
            TeamRemovals = removals,
            OtherCharacters = otherCharacters,
            Owner = await dcp.GetMemberDtoAsync(character.OwnerId),
            VerifiedBy = await dcp.GetMemberDtoAsync(character.VerifiedById)
        };
    }

    [HttpDelete("{id:long}/Removals/{removalId:long}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> DeleteRemoval(long id, long removalId)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        var removal = await _context.TeamRemovals.FindAsync(removalId);

        if (removal is null)
        {
            return NotFound();
        }

        if (removal.CharacterId != character.Id)
        {
            return Problem("Removal is not for the specified character.");
        }

        // Other removal relationships will be set to null on save. Attendances need to also have their ignore fields cleared.
        await foreach (var attendance in _context.RaidAttendees.AsTracking().Where(a => a.RemovalId == removal.Id).AsAsyncEnumerable())
        {
            attendance.Removal = null;
            attendance.RemovalId = null;
        }

        _context.TeamRemovals.Remove(removal);

        _context.TeamMembers.Add(new TeamMember
        {
            JoinedAt = removal.JoinedAt,
            CharacterId = character.Id,
            Disenchanter = false,
            Enchanted = false,
            Prepared = false,
            TeamId = removal.TeamId
        });

        await _context.SaveChangesAsync();

        TrackTelemetry("RemovalUndone", character);

        return Ok();
    }

    [HttpPost("{id:long}/Verify"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> PostVerify(long id)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        if (character.VerifiedById.HasValue)
        {
            return Problem("Character is already verified.", statusCode: 400);
        }

        character.VerifiedById = User.GetDiscordId();

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterVerified", character);

        return Ok();
    }

    [HttpPut("{id:long}/OwnerId"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> SetOwner(long id, [FromBody] long ownerId, [FromServices] DiscordClientProvider dcp)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        var member = await dcp.GetMemberAsync(ownerId);

        if (member is null)
        {
            return Problem("No user with that id exists.");
        }

        character.OwnerId = ownerId;
        character.VerifiedById = User.GetDiscordId();

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterOwnerSet", character);

        return Ok();
    }

    [HttpDelete("{id:long}/OwnerId"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> DeleteOwner(long id)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        character.OwnerId = null;
        character.VerifiedById = null;

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterOwnerUnset", character);

        return Ok();
    }

    [HttpGet("{id:long}/Attendances")]
    public IAsyncEnumerable<CharacterAttendanceDto> GetAttendances(long id)
    {
        return _context.RaidAttendees
            .AsNoTracking()
            .Where(ra => ra.CharacterId == id)
            .OrderByDescending(ra => ra.Raid.StartedAt)
            .Select(ra => new CharacterAttendanceDto
            {
                RaidId = ra.RaidId,
                StartedAt = ra.Raid.StartedAt,
                TeamId = ra.Raid.RaidTeamId,
                TeamName = ra.Raid.RaidTeam.Name
            })
            .AsAsyncEnumerable();
    }

    [HttpPost("{id:long}/ToggleActivated"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> ToggleHidden(long id)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        if (!character.Deactivated && await _context.TeamMembers.AnyAsync(tm => tm.CharacterId == id))
        {
            return Problem("Can't deactivate a character who is on a raid team.");
        }

        character.Deactivated = !character.Deactivated;

        await _context.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id:long}"), Authorize(AppPolicies.Administrator)]
    public async Task<ActionResult> Delete(long id)
    {
        var character = await _context.Characters.FindAsync(id);

        if (character is null)
        {
            return NotFound();
        }

        if (await _context.Drops.CountAsync(d => d.WinnerId == id || d.WinningEntry!.LootList.CharacterId == id) > 0)
        {
            return Problem("Can't delete a character who has already won loot.");
        }

        if (await _context.RaidAttendees.CountAsync(a => a.CharacterId == id) > 0)
        {
            return Problem("Can't delete a character who has already attended a raid.");
        }

        _context.Characters.Remove(character);

        await _context.SaveChangesAsync();

        TrackTelemetry("CharacterDeleted", character);

        return Ok();
    }

    [HttpGet("{id:long}/Specs")]
    public async Task<ActionResult<IEnumerable<Specializations>>> GetSpecs(long id)
    {
        var character = await _context.Characters
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new { c.Class })
            .FirstOrDefaultAsync();

        if (character is null)
        {
            return NotFound();
        }

        return (Specializations[])(character.Class switch
        {
            Classes.Druid => [Specializations.BalanceDruid, Specializations.BearDruid, Specializations.CatDruid, Specializations.RestoDruid],
            Classes.Hunter => [Specializations.BeastMasterHunter, Specializations.MarksmanHunter, Specializations.SurvivalHunter],
            Classes.Mage => [Specializations.ArcaneMage, Specializations.FireMage, Specializations.FrostMage],
            Classes.Paladin => [Specializations.HolyPaladin, Specializations.ProtPaladin, Specializations.RetPaladin],
            Classes.Priest => [Specializations.DiscPriest, Specializations.HolyPriest, Specializations.ShadowPriest],
            Classes.Rogue => [Specializations.AssassinationRogue, Specializations.CombatRogue, Specializations.SubtletyRogue],
            Classes.Shaman => [Specializations.EleShaman, Specializations.EnhanceShaman, Specializations.RestoShaman],
            Classes.Warlock => [Specializations.AfflictionWarlock, Specializations.DemoWarlock, Specializations.DestroWarlock],
            Classes.Warrior => [Specializations.ArmsWarrior, Specializations.FuryWarrior, Specializations.ProtWarrior],
            Classes.DeathKnight => [Specializations.BloodDeathKnight, Specializations.BloodDeathKnightTank, Specializations.FrostDeathKnight, Specializations.FrostDeathKnightTank, Specializations.UnholyDeathKnight, Specializations.UnholyDeathKnightTank],
            _ => []
        });
    }

    private void TrackTelemetry(string name, Character character)
    {
        _telemetry.TrackEvent(name, User, properties =>
        {
            properties["CharacterId"] = character.Id.ToString();
            properties["Character"] = character.Name;
        });
    }
}
