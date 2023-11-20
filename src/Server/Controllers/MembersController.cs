// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers;

public class MembersController(DiscordClientProvider discordClientProvider) : ApiControllerV1
{
    private readonly DiscordClientProvider _discordClientProvider = discordClientProvider;

    [HttpGet, Authorize(AppPolicies.Administrator)]
    public async Task<IEnumerable<GuildMemberDto>> Get([FromQuery] string[]? role, bool force = false)
    {
        var guild = await _discordClientProvider.GetGuildAsync();

        IEnumerable<DiscordMember> members;

        if (force)
        {
            members = await guild.GetAllMembersAsync();
        }
        else
        {
            members = guild.Members.Values;
        }

        if (role?.Length > 0)
        {
            members = members.Where(m => role.Any(r => _discordClientProvider.HasAppRole(m, r)));
        }

        return members.Select(_discordClientProvider.CreateDto);
    }
}
