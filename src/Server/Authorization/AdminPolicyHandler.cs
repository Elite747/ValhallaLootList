﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Authorization;

public class AdminPolicyHandler(DiscordClientProvider discordClientProvider) : DiscordAuthorizationHandler<AdminRequirement>(discordClientProvider)
{
    protected override ValueTask HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement, DiscordMember member)
    {
        if (DiscordClientProvider.HasAdminRole(member))
        {
            context.Succeed(requirement);
        }
        return default;
    }
}
