// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class InstancesController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public InstancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<InstanceDto> Get(byte? phase, byte? minPhase, byte? maxPhase, bool includeEncounters = true)
        {
            var query = _context.Instances.AsNoTracking();

            if (phase.HasValue)
            {
                query = query.Where(i => i.Phase == phase.Value).OrderBy(i => i.Name);
            }
            else
            {
                if (minPhase.HasValue)
                {
                    query = query.Where(i => i.Phase >= minPhase.Value);
                }
                if (maxPhase.HasValue)
                {
                    query = query.Where(i => i.Phase <= maxPhase.Value);
                }

                query = query.OrderBy(i => i.Phase).ThenBy(i => i.Name);
            }

            if (!includeEncounters)
            {
                return query
                    .Select(i => new InstanceDto
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Phase = i.Phase,
                    })
                    .AsAsyncEnumerable();
            }

            return query
                .Select(i => new InstanceDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Phase = i.Phase,
                    Encounters = i.Encounters.OrderBy(e => e.Index).Select(e => new EncounterDto
                    {
                        Id = e.Id,
                        Items = e.Items.Where(item => item.RewardFromId == null).Select(item => item.Id).ToList(),
                        Name = e.Name
                    }).ToList()
                })
                .AsSingleQuery()
                .AsAsyncEnumerable();
        }
    }
}
