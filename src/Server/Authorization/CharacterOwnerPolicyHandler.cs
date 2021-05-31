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

            long characterId;
            long discordId = (long)member.Id;

            switch (context.Resource)
            {
                case long l:
                    characterId = l;
                    break;
                case CharacterDto dto:
                    characterId = dto.Id;
                    break;
                case Character character:
                    if (character.OwnerId == discordId)
                    {
                        context.Succeed(requirement);
                    }
                    return;
                default:
                    return;
            }

            if (await _context.Characters.AsNoTracking().CountAsync(c => c.Id == characterId && c.OwnerId == discordId) > 0)
            {
                context.Succeed(requirement);
            }
        }
    }
}
