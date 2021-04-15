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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    public class CharactersController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;
        private readonly TelemetryClient _telemetry;

        public CharactersController(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo, TelemetryClient telemetry)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
            _telemetry = telemetry;
        }

        [HttpGet]
        public IAsyncEnumerable<CharacterDto> Get(string? team = null)
        {
            var query = _context.Characters.AsNoTracking();

            if (team?.Length > 0)
            {
                if (string.Equals(team, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    query = query.Where(c => c.TeamId == null);
                }
                else if (long.TryParse(team, out var teamId))
                {
                    query = query.Where(c => c.TeamId == teamId);
                }
                else
                {
                    query = query.Where(c => c.Team!.Name == team);
                }
            }

            return query
                .OrderBy(c => c.Name)
                .Select(c => new CharacterDto
                {
                    Class = c.Class,
                    Id = c.Id,
                    Name = c.Name,
                    Race = c.Race,
                    TeamId = c.TeamId,
                    TeamName = c.Team!.Name,
                    Gender = c.IsFemale ? Gender.Female : Gender.Male
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
        public async Task<ActionResult<IEnumerable<CharacterDto>>> GetMine()
        {
            var userId = User.GetDiscordId();
            var characterIds = await _context.UserClaims
                .AsNoTracking()
                .Where(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.Character)
                .Select(claim => claim.ClaimValue)
                .ToListAsync();

            var ids = new HashSet<long>();

            foreach (var characterIdString in characterIds)
            {
                if (long.TryParse(characterIdString, out var characterId))
                {
                    ids.Add(characterId);
                }
            }

            return await _context.Characters
                .AsNoTracking()
                .Where(c => ids.Contains(c.Id))
                .OrderBy(c => c.Name)
                .Select(c => new CharacterDto
                {
                    Class = c.Class,
                    Id = c.Id,
                    Name = c.Name,
                    Race = c.Race,
                    TeamId = c.TeamId,
                    TeamName = c.Team!.Name,
                    Gender = c.IsFemale ? Gender.Female : Gender.Male
                })
                .ToListAsync();
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
                    TeamId = c.Team!.Id,
                    TeamName = c.Team.Name,
                    Gender = c.IsFemale ? Gender.Female : Gender.Male
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
            Debug.Assert(dto.Class.HasValue);
            Debug.Assert(dto.Race.HasValue);

            var normalizedName = NameHelpers.NormalizeName(dto.Name);

            if (await _context.Characters.AsNoTracking().AnyAsync(c => c.Name.Equals(normalizedName)))
            {
                ModelState.AddModelError(nameof(dto.Name), "A character with that name already exists.");
                return ValidationProblem();
            }

            if (!dto.Race.Value.IsValidRace())
            {
                ModelState.AddModelError(nameof(dto.Race), "Race selection is not valid.");
            }

            if (!dto.Class.Value.IsSingleClass())
            {
                ModelState.AddModelError(nameof(dto.Class), "Class selection is not valid.");
                return ValidationProblem();
            }

            if ((dto.Class.Value & dto.Race.Value.GetClasses()) == 0)
            {
                ModelState.AddModelError(nameof(dto.Class), "Class is not available to the selected race.");
                return ValidationProblem();
            }

            var character = new Character(idGenerator.CreateId())
            {
                Class = dto.Class.Value,
                MemberStatus = RaidMemberStatus.FullTrial,
                Name = normalizedName,
                Race = dto.Race.Value,
                IsFemale = dto.Gender == Gender.Female
            };

            if (dto.SenderIsOwner)
            {
                var userId = User.GetDiscordId();

                Debug.Assert(userId.HasValue);

                _context.UserClaims.Add(new() { UserId = userId.Value, ClaimType = AppClaimTypes.Character, ClaimValue = character.Id.ToString() });

                if (User.IsAdmin())
                {
                    character.VerifiedById = userId;
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
                Gender = character.IsFemale ? Gender.Female : Gender.Male
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

            if (dto.Name?.Length > 0)
            {
                character.Name = dto.Name;
            }

            if (dto.Race.HasValue)
            {
                character.Race = dto.Race.Value;
            }

            if (dto.Gender.HasValue)
            {
                character.IsFemale = dto.Gender == Gender.Female;
            }

            await _context.SaveChangesAsync();

            TrackTelemetry("CharacterUpdated", character);

            return new CharacterDto
            {
                Class = character.Class,
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Gender = character.IsFemale ? Gender.Female : Gender.Male,
                TeamId = character.TeamId
            };
        }

        [HttpGet("{id:long}/Admin"), Authorize(AppPolicies.Administrator)]
        public async Task<ActionResult<CharacterAdminDto>> GetAdmin(long id, [FromServices] DiscordService discordService)
        {
            var character = await _context.Characters
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new { c.VerifiedById })
                .FirstOrDefaultAsync();

            if (character is null)
            {
                return NotFound();
            }

            var idString = id.ToString();

            var claim = await _context.UserClaims.AsNoTracking()
                .Where(c => c.ClaimType == AppClaimTypes.Character && c.ClaimValue == idString)
                .Select(c => new { c.UserId })
                .FirstOrDefaultAsync();

            var removals = await _context.TeamRemovals
                .AsNoTracking()
                .Where(r => r.CharacterId == id)
                .OrderByDescending(r => r.RemovedAt)
                .Select(r => new TeamRemovalDto
                {
                    Id = r.Id,
                    RemovedAt = r.RemovedAt,
                    TeamId = r.TeamId,
                    TeamName = r.Team.Name
                })
                .ToListAsync();

            return new CharacterAdminDto
            {
                TeamRemovals = removals,
                Owner = await discordService.GetGuildMemberDtoAsync(claim?.UserId),
                VerifiedBy = await discordService.GetGuildMemberDtoAsync(character.VerifiedById)
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
                attendance.IgnoreAttendance = false;
                attendance.IgnoreReason = null;
                attendance.Removal = null;
                attendance.RemovalId = null;
            }

            _context.TeamRemovals.Remove(removal);

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
        public async Task<ActionResult> SetOwner(long id, [FromBody] long ownerId)
        {
            var character = await _context.Characters.FindAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(ownerId);

            if (user is null)
            {
                return Problem("No user with that id exists.");
            }

            var idString = id.ToString();

            var existingClaims = await _context.UserClaims
                .AsTracking()
                .Where(claim => claim.ClaimType == AppClaimTypes.Character && claim.ClaimValue == idString)
                .ToListAsync();

            _context.UserClaims.RemoveRange(existingClaims);
            _context.UserClaims.Add(new() { ClaimType = AppClaimTypes.Character, ClaimValue = idString, UserId = ownerId });
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

            var idString = id.ToString();

            var existingClaims = await _context.UserClaims
                .AsTracking()
                .Where(claim => claim.ClaimType == AppClaimTypes.Character && claim.ClaimValue == idString)
                .ToListAsync();

            _context.UserClaims.RemoveRange(existingClaims);

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
                    IgnoreAttendance = ra.IgnoreAttendance,
                    IgnoreReason = ra.IgnoreReason,
                    RaidId = ra.RaidId,
                    StartedAt = ra.Raid.StartedAt,
                    TeamId = ra.Raid.RaidTeamId,
                    TeamName = ra.Raid.RaidTeam.Name
                })
                .AsAsyncEnumerable();
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

            return character.Class switch
            {
                Classes.Druid => new[] { Specializations.BalanceDruid, Specializations.BearDruid, Specializations.CatDruid, Specializations.RestoDruid },
                Classes.Hunter => new[] { Specializations.Hunter },
                Classes.Mage => new[] { Specializations.Mage },
                Classes.Paladin => new[] { Specializations.HolyPaladin, Specializations.ProtPaladin, Specializations.RetPaladin },
                Classes.Priest => new[] { Specializations.HealerPriest, Specializations.ShadowPriest },
                Classes.Rogue => new[] { Specializations.Rogue },
                Classes.Shaman => new[] { Specializations.EleShaman, Specializations.EnhanceShaman, Specializations.RestoShaman },
                Classes.Warlock => new[] { Specializations.Warlock },
                Classes.Warrior => new[] { Specializations.ArmsWarrior, Specializations.FuryWarrior, Specializations.ProtWarrior },
                _ => Array.Empty<Specializations>()
            };
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
}
