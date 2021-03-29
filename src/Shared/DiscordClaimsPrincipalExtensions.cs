// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Security.Claims;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList
{
    public static class DiscordClaimsPrincipalExtensions
    {
        public static string? GetDiscordAvatarHash(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(DiscordClaimTypes.AvatarHash)?.Value;
        }

        public static Uri? GetDiscordAvatarUrl(this ClaimsPrincipal principal)
        {
            if (Uri.TryCreate(principal.FindFirst(DiscordClaimTypes.AvatarUrl)?.Value, UriKind.Absolute, out var result))
            {
                return result;
            }
            return null;
        }

        public static int? GetDiscordDiscriminator(this ClaimsPrincipal principal)
        {
            if (int.TryParse(principal.FindFirst(DiscordClaimTypes.Discriminator)?.Value, out var result))
            {
                return result;
            }
            return null;
        }

        public static string? GetDiscordUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(DiscordClaimTypes.Username)?.Value;
        }

        public static string? GetAppUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public static long? GetDiscordId(this ClaimsPrincipal principal)
        {
            return GetDiscordId(principal, ClaimTypes.NameIdentifier);
        }

        public static long? GetDiscordIdFromClient(this ClaimsPrincipal principal)
        {
            return GetDiscordId(principal, "sub");
        }

        private static long? GetDiscordId(this ClaimsPrincipal principal, string claim)
        {
            if (long.TryParse(principal.FindFirst(claim)?.Value, out var result))
            {
                return result;
            }
            return null;
        }

        public static GuildMemberDto? CreateGuildMemberFromClient(this ClaimsPrincipal principal)
        {
            var id = GetDiscordIdFromClient(principal);
            if (id.HasValue)
            {
                return CreateGuildMember(principal, id.Value);
            }
            return null;
        }

        public static GuildMemberDto? CreateGuildMemberFromServer(this ClaimsPrincipal principal)
        {
            var id = GetDiscordId(principal);
            if (id.HasValue)
            {
                return CreateGuildMember(principal, id.Value);
            }
            return null;
        }

        private static GuildMemberDto CreateGuildMember(ClaimsPrincipal principal, long id)
        {
            return new GuildMemberDto
            {
                Avatar = principal.GetDiscordAvatarHash(),
                Discriminator = principal.FindFirst(DiscordClaimTypes.Discriminator)?.Value ?? string.Empty,
                Id = id,
                Nickname = principal.Identity?.Name ?? principal.GetDiscordUsername() ?? string.Empty,
                Username = principal.GetDiscordUsername() ?? string.Empty
            };
        }

        public static bool IsAdmin(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(AppClaimTypes.Role, AppRoles.Administrator);
        }

        public static bool IsMember(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(AppClaimTypes.Role, AppRoles.Member);
        }

        public static bool IsRaidLeader(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(AppClaimTypes.Role, AppRoles.RaidLeader);
        }

        public static bool IsLootMaster(this ClaimsPrincipal principal)
        {
            return principal.HasClaim(AppClaimTypes.Role, AppRoles.LootMaster);
        }

        public static bool IsLeaderOf(this ClaimsPrincipal principal, long teamId)
        {
            return principal.HasClaim(AppClaimTypes.RaidLeader, teamId.ToString());
        }

        public static bool IsOwnerOf(this ClaimsPrincipal principal, long characterId)
        {
            return principal.HasClaim(AppClaimTypes.Character, characterId.ToString());
        }
    }
}
