// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class InstancesController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;

    public InstancesController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async IAsyncEnumerable<InstanceDto> Get()
    {
        var items = await _context.EncounterItems.Select(item => new { item.ItemId, item.Is25, item.Heroic, item.EncounterId }).ToListAsync();
        var encounters = await _context.Encounters.Select(e => new { e.Id, e.Index, e.InstanceId, e.Name, e.Phase }).ToListAsync();

        await foreach (var instance in _context.Instances.AsNoTracking()
            .OrderBy(i => i.Phase)
            .ThenBy(i => i.Name)
            .Select(i => new InstanceDto { Id = i.Id, Name = i.Name, Phase = i.Phase })
            .AsAsyncEnumerable())
        {
            foreach (var encounter in encounters
                .Where(e => e.InstanceId == instance.Id)
                .OrderBy(e => e.Index)
                .Select(e => new EncounterDto { Id = e.Id, IsTrash = e.Index < 0, Name = e.Name, Phase = e.Phase }))
            {
                encounter.Variants = items
                    .Where(i => i.EncounterId == encounter.Id)
                    .GroupBy(i => new { i.Is25, i.Heroic })
                    .Select(g => new EncounterVariant
                    {
                        Heroic = g.Key.Heroic,
                        Size = g.Key.Is25 ? 25 : 10,
                        Items = g.Select(i => i.ItemId).ToList()
                    })
                    .ToList();

                instance.Encounters.Add(encounter);
            }

            yield return instance;
        }
    }
}
