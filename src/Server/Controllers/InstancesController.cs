// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InstancesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InstancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<InstanceDto> Get(byte? phase)
        {
            var query = _context.Instances.AsNoTracking();

            if (phase.HasValue)
            {
                query = query.Where(i => i.Phase == phase.Value).OrderBy(i => i.Name);
            }
            else
            {
                query = query.OrderBy(i => i.Phase).ThenBy(i => i.Name);
            }

            return query
                .Select(i => new InstanceDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Phase = i.Phase,
                    Encounters = i.Encounters.Select(e => new EncounterDto
                    {
                        Id = e.Id,
                        Items = e.Items.Where(item => item.RewardFromId == null).Select(item => item.Id).ToList(),
                        Name = e.Name
                    }).ToList()
                })
                .AsAsyncEnumerable();
        }
    }
}
