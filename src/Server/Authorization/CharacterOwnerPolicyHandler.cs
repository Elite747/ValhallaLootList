// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Authorization
{
    public class CharacterOwnerPolicyHandler : DiscordAuthorizationHandler<CharacterOwnerRequirement>
    {
        private readonly ApplicationDbContext _context;

        public CharacterOwnerPolicyHandler(ApplicationDbContext context, DiscordClientProvider discordClientProvider) : base(discordClientProvider)
        {
            _context = context;
        }

        protected override async ValueTask HandleRequirementAsync(AuthorizationHandlerContext context, CharacterOwnerRequirement requirement, DiscordMember member)
        {
            if (requirement.AllowAdmin && DiscordClientProvider.HasAdminRole(member))
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
                var discordId = (long)member.Id;
                var characterIdString = characterId.Value.ToString();

                if (context.User.HasClaim(AppClaimTypes.Character, characterIdString) ||
                    await _context.UserClaims.AsNoTracking().CountAsync(claim => claim.UserId == discordId && claim.ClaimType == AppClaimTypes.Character && claim.ClaimValue == characterIdString) > 0)
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
