// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Security.Claims;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList;

public static class DiscordClaimsPrincipalExtensions
{
    public static long? GetDiscordId(this ClaimsPrincipal principal)
    {
        foreach (var claim in principal.Claims)
        {
            if (string.Equals(claim.Type, AppClaimTypes.Id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(claim.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))
            {
                if (long.TryParse(claim.Value, out var id))
                {
                    return id;
                }
            }
        }

        return null;
    }

    public static GuildMemberDto CreateGuildMember(this ClaimsPrincipal principal)
    {
        var member = new GuildMemberDto();

        foreach (var claim in principal.Claims)
        {
            if (string.Equals(claim.Type, DiscordClaimTypes.AvatarHash, StringComparison.OrdinalIgnoreCase))
            {
                member.Avatar = claim.Value;
            }
            else if (string.Equals(claim.Type, DiscordClaimTypes.Discriminator, StringComparison.OrdinalIgnoreCase))
            {
                member.Discriminator = claim.Value;
            }
            else if (string.Equals(claim.Type, DiscordClaimTypes.Nickname, StringComparison.OrdinalIgnoreCase))
            {
                member.Nickname = claim.Value;
            }
            else if (string.Equals(claim.Type, DiscordClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            {
                member.DiscordRoles.Add(claim.Value);
            }
            else if (string.Equals(claim.Type, DiscordClaimTypes.Username, StringComparison.OrdinalIgnoreCase))
            {
                member.Username = claim.Value;
            }
            else if (string.Equals(claim.Type, AppClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
            {
                member.AppRoles.Add(claim.Value);
            }
            else if ((string.Equals(claim.Type, AppClaimTypes.Id, StringComparison.OrdinalIgnoreCase)
                || string.Equals(claim.Type, ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))
                && long.TryParse(claim.Value, out var discordId))
            {
                member.Id = discordId;
            }
        }

        return member;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal)
    {
        string? name = null;

        foreach (var claim in principal.Claims)
        {
            if (string.Equals(claim.Type, DiscordClaimTypes.Nickname, StringComparison.OrdinalIgnoreCase) && claim.Value?.Length > 0)
            {
                return claim.Value;
            }
            if (string.Equals(claim.Type, DiscordClaimTypes.Username, StringComparison.OrdinalIgnoreCase))
            {
                name = claim.Value;
            }
        }

        if (name?.Length > 0)
        {
            return name;
        }

        throw new ArgumentException("Principal does not contain any discord name claims.", nameof(principal));
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.HasClaim(AppClaimTypes.Role, AppRoles.Administrator);
    }

    public static bool IsLeadership(this ClaimsPrincipal principal)
    {
        return principal.Claims.Any(claim => claim.Type == AppClaimTypes.Role && claim.Value is AppRoles.Administrator or AppRoles.RaidLeader or AppRoles.LootMaster or AppRoles.Recruiter);
    }
}
