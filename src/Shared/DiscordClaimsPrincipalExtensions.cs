// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Security.Claims;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList
{
    public static class DiscordClaimsPrincipalExtensions
    {
        public static long? GetDiscordId(this ClaimsPrincipal principal)
        {
            Claim? claim = principal.FindFirst("sub") ?? principal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim is not null && long.TryParse(claim.Value, out var result))
            {
                return result;
            }
            return null;
        }

        public static GuildMemberDto? CreateGuildMember(this ClaimsPrincipal principal)
        {
            long? id = GetDiscordId(principal);
            string? username = principal.FindFirst(DiscordClaimTypes.Username)?.Value;
            if (id.HasValue && username?.Length > 0)
            {
                return new GuildMemberDto
                {
                    Avatar = principal.FindFirst(DiscordClaimTypes.AvatarHash)?.Value,
                    Discriminator = principal.FindFirst(DiscordClaimTypes.Discriminator)?.Value ?? string.Empty,
                    Id = id.Value,
                    Nickname = principal.Identity?.Name ?? username,
                    Username = username,
                    AppRoles = principal.FindAll(AppClaimTypes.Role).Select(claim => claim.Value).ToList(),
                    DiscordRoles = principal.FindAll(DiscordClaimTypes.Role).Select(claim => claim.Value).ToList()
                };
            }
            return null;
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(AppClaimTypes.Role, AppRoles.Administrator);
        }
    }
}
