// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    public class CharactersController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;

        public CharactersController(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
        }

        [HttpGet]
        public IAsyncEnumerable<CharacterDto> Get(bool owned = false, string? team = null)
        {
            var currentUserId = User.GetDiscordId();
            bool isAdmin = User.IsAdmin();

            var query = _context.Characters.AsNoTracking();

            if (owned)
            {
                query = query.Where(c => c.OwnerId == currentUserId);
            }

            if (team?.Length > 0)
            {
                if (string.Equals(team, "none", StringComparison.InvariantCultureIgnoreCase))
                {
                    query = query.Where(c => c.TeamId == null);
                }
                else
                {
                    query = query.Where(c => c.TeamId == team || c.Team!.Name == team);
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
                    Gender = c.IsMale ? Gender.Male : Gender.Female,
                    Editable = isAdmin || c.OwnerId == currentUserId
                })
                .AsAsyncEnumerable();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CharacterDto>> Get(string id)
        {
            var currentUserId = User.GetDiscordId();
            bool isAdmin = User.IsAdmin();

            var character = await _context.Characters
                .AsNoTracking()
                .Where(c => c.Id == id || c.Name.Equals(id, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CharacterDto
                {
                    Class = c.Class,
                    Id = c.Id,
                    Name = c.Name,
                    Race = c.Race,
                    TeamId = c.Team!.Id,
                    TeamName = c.Team.Name,
                    Gender = c.IsMale ? Gender.Male : Gender.Female,
                    Editable = isAdmin || c.OwnerId == currentUserId
                })
                .FirstOrDefaultAsync();

            if (character is null)
            {
                return NotFound();
            }

            return character;
        }

        [HttpPost]
        public async Task<ActionResult<CharacterDto>> Post([FromBody] CharacterSubmissionDto dto)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem();
            }

            Debug.Assert(dto.Name?.Length > 1);
            Debug.Assert(dto.Class.HasValue);
            Debug.Assert(dto.Race.HasValue);

            var normalizedName = NormalizeName(dto.Name);

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

            var currentUserId = User.GetDiscordId();

            var character = new Character
            {
                Class = dto.Class.Value,
                MemberStatus = RaidMemberStatus.FullTrial,
                VerifiedById = User.IsAdmin() ? currentUserId : null,
                Name = normalizedName,
                Race = dto.Race.Value,
                IsMale = dto.Gender == Gender.Male,
                OwnerId = currentUserId
            };

            _context.Characters.Add(character);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = character.Id }, new CharacterDto
            {
                Class = character.Class,
                Id = character.Id,
                Name = character.Name,
                Race = character.Race,
                Gender = character.IsMale ? Gender.Male : Gender.Female
            });
        }

        [HttpGet("{id}/Owner"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult<CharacterOwnerDto>> GetOwner(string id, [FromServices] DiscordService discordService)
        {
            var character = await _context.Characters.Where(c => c.Id == id).Select(c => new { c.OwnerId, c.VerifiedById }).FirstOrDefaultAsync();

            if (character is null)
            {
                return NotFound();
            }

            return new CharacterOwnerDto
            {
                Owner = await GetGuildMemberDtoAsync(character.OwnerId, discordService),
                VerifiedBy = await GetGuildMemberDtoAsync(character.VerifiedById, discordService)
            };
        }

        private static async Task<GuildMemberDto?> GetGuildMemberDtoAsync(long? id, DiscordService discordService)
        {
            if (id.HasValue)
            {
                var guildMember = await discordService.GetMemberAsync(id.Value);

                if (guildMember?.User is not null)
                {
                    return new GuildMemberDto
                    {
                        Discriminator = guildMember.User.Discriminator,
                        Id = guildMember.User.Id,
                        Nickname = guildMember.Nickname,
                        Username = guildMember.User.Username
                    };
                }
            }

            return null;
        }

        [HttpPost("{id}/Verify"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult> PostVerify(string id)
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

            return Ok();
        }

        [HttpPut("{id}/OwnerId"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult> SetOwner(string id, [FromBody] string ownerId)
        {
            var character = await _context.Characters.FindAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(ownerId);

            if (user is null)
            {
                return Problem("No user with that id exists.", statusCode: 400);
            }

            character.OwnerId = long.Parse(user.Id);
            character.VerifiedById = User.GetDiscordId();

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}/OwnerId"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult> DeleteOwner(string id)
        {
            var character = await _context.Characters.FindAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            character.OwnerId = null;
            character.VerifiedById = null;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}"), Authorize(AppRoles.Administrator)]
        public async Task<ActionResult> Delete(string id)
        {
            var character = await _context.Characters.FindAsync(id);

            if (character is null)
            {
                return NotFound();
            }

            _context.Characters.Remove(character);

            await _context.SaveChangesAsync();

            return Ok();
        }

        private static string NormalizeName(string name)
        {
            return string.Create(name.Length, name, (span, name) =>
            {
                span[0] = char.ToUpperInvariant(name[0]);

                for (int i = 1; i < span.Length; i++)
                {
                    span[i] = char.ToLowerInvariant(name[i]);
                }
            });
        }
    }
}
