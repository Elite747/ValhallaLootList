// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class ConfigController(ApplicationDbContext context) : ApiControllerV1
{
    private readonly ApplicationDbContext _context = context;

    [HttpGet("phases")]
    public async Task<ActionResult<PhaseConfigDto>> GetPhaseConfig()
    {
        var dto = new PhaseConfigDto();
        var now = DateTimeOffset.UtcNow;
        bool currentPhaseFound = false;

        await foreach (var phase in _context.PhaseDetails.AsNoTracking().OrderByDescending(p => p.Id).AsAsyncEnumerable())
        {
            if (!currentPhaseFound && phase.StartsAt <= now)
            {
                dto.CurrentPhase = phase.Id;
                currentPhaseFound = true;
            }
            dto.Phases.Add(phase.Id);
        }

        return dto;
    }
}
