// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server;

public class IdentityProfileService(DiscordClientProvider discordClientProvider, ILogger<DefaultProfileService> logger) : DefaultProfileService(logger)
{
    private readonly DiscordClientProvider _discordClientProvider = discordClientProvider;

    public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        context.LogProfileRequest(Logger);

        if (long.TryParse(context.Subject.GetSubjectId(), out var userId))
        {
            var member = await _discordClientProvider.GetMemberAsync(userId);

            if (member is not null)
            {
                // add discord claims
                TryAddClaim(context, DiscordClaimTypes.AvatarHash, member.AvatarHash);
                TryAddClaim(context, DiscordClaimTypes.AvatarUrl, member.AvatarUrl);
                TryAddClaim(context, DiscordClaimTypes.Discriminator, member.Discriminator);
                TryAddClaim(context, DiscordClaimTypes.Username, member.Username);
                TryAddClaim(context, DiscordClaimTypes.Nickname, member.Nickname);

                // add discord role claims
                foreach (var role in member.Roles)
                {
                    TryAddClaim(context, DiscordClaimTypes.Role, role.Name);
                }

                // add app role claims
                foreach (var appRole in _discordClientProvider.GetAppRoles(member))
                {
                    TryAddClaim(context, AppClaimTypes.Role, appRole);
                }
            }
        }

        context.LogIssuedClaims(Logger);
    }

    private static void TryAddClaim(ProfileDataRequestContext context, string claimType, string? value)
    {
        if (value?.Length > 0)
        {
            context.IssuedClaims.Add(new(claimType, value));
        }
    }

    public override async Task IsActiveAsync(IsActiveContext context)
    {
        context.IsActive = false;

        if (long.TryParse(context.Subject.GetSubjectId(), out var id))
        {
            var guildMember = await _discordClientProvider.GetMemberAsync(id);

            if (guildMember is not null)
            {
                context.IsActive = _discordClientProvider.HasMemberRole(guildMember);
            }
        }
    }
}
