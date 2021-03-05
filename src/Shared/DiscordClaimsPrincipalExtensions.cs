﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Security.Claims;

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
            if (long.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var result))
            {
                return result;
            }
            return null;
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
    }
}
