// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class EncountersController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;

    public EncountersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public ActionResult<IAsyncEnumerable<EncounterDto>> Get(string? instance, string? instanceId)
    {
        var query = _context.Encounters.AsNoTracking();

        if (instance?.Length > 0)
        {
            if (instanceId?.Length > 0)
            {
                return Problem("Either instance or instanceId must be specified, but not both.", statusCode: 400);
            }

            query = query.Where(e => e.Instance.Name == instance);
        }
        else if (instanceId?.Length > 0)
        {
            query = query.Where(e => e.InstanceId == instanceId);
        }
        else
        {
            return Problem("Either instance or instanceId must be specified.", statusCode: 400);
        }

        return Ok(query
            .OrderBy(e => e.Index)
            .Select(e => new EncounterDto
            {
                Id = e.Id,
                Name = e.Name,
                IsTrash = e.Index < 0,
                Items = e.Items.Where(item => item.Item.RewardFromId == null).Select(item => item.ItemId).ToList()
            })
            .AsAsyncEnumerable());
    }
}
