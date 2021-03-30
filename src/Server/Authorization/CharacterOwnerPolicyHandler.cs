// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Authorization
{
    public class CharacterOwnerPolicyHandler : AuthorizationHandler<CharacterOwnerRequirement>
    {
        private readonly ApplicationDbContext _context;

        public CharacterOwnerPolicyHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CharacterOwnerRequirement requirement)
        {
            if (context.User.IsAdmin() && requirement.AllowAdmin)
            {
                context.Succeed(requirement);
                return;
            }

            long? characterId = context.Resource switch
            {
                long id => id,
                CharacterDto character => character.Id,
                _ => null
            };

            if (characterId.HasValue)
            {
                if (context.User.IsOwnerOf(characterId.Value))
                {
                    context.Succeed(requirement);
                    return;
                }

                var userId = context.User.GetDiscordId();
                var idString = characterId.ToString();

                if (userId.HasValue && await _context.UserClaims.AsNoTracking().AnyAsync(claim => claim.UserId == userId && claim.ClaimType == AppClaimTypes.Character && claim.ClaimValue == idString))
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
