// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Authorization;

public abstract class DiscordAuthorizationHandler<TRequirement> : AuthorizationHandler<TRequirement> where TRequirement : IAuthorizationRequirement
{
    protected DiscordAuthorizationHandler(DiscordClientProvider discordClientProvider)
    {
        DiscordClientProvider = discordClientProvider ?? throw new System.ArgumentNullException(nameof(discordClientProvider));
    }

    protected DiscordClientProvider DiscordClientProvider { get; }

    protected sealed override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        var discordId = context.User.GetDiscordId() ?? default;

        if (discordId == 0)
        {
            context.Fail();
            return;
        }

        var member = await DiscordClientProvider.GetMemberAsync(discordId);

        if (member is null)
        {
            context.Fail();
            return;
        }

        if (DiscordClientProvider.HasMemberRole(member))
        {
            await HandleRequirementAsync(context, requirement, member);
        }
    }

    protected abstract ValueTask HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, DiscordMember member);
}
