// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class PermissionsController(ApplicationDbContext context) : ApiControllerV1
{
    private readonly ApplicationDbContext _context = context;

    public async Task<PermissionsDto> Get()
    {
        var dto = new PermissionsDto();
        var userId = User.GetDiscordId();
        Debug.Assert(userId.HasValue);

        await foreach (var characterId in _context.Characters
            .AsNoTracking()
            .Where(c => c.OwnerId == userId)
            .Select(c => c.Id)
            .AsAsyncEnumerable())
        {
            dto.Characters.Add(characterId);
        }

        await foreach (var teamId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.UserId == userId)
            .Select(rtl => rtl.RaidTeamId)
            .AsAsyncEnumerable())
        {
            dto.Teams.Add(teamId);
        }

        return dto;
    }
}
