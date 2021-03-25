// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class ItemsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<ItemDto> Get(byte? phase, Specializations? spec, bool includeTokens = false)
        {
            var query = _context.Items.AsNoTracking();

            if (phase.HasValue)
            {
                query = query.Where(item => item.Phase == phase.Value);
            }

            if (!includeTokens)
            {
                query = query.Where(item => item.Slot != InventorySlot.Unknown);
            }

            if (spec.HasValue && spec != Specializations.All)
            {
                query = query.Where(item => !item.Restrictions.Any(r => (r.Specializations & spec.Value) != 0 && r.RestrictionLevel == ItemRestrictionLevel.Unequippable));
            }

            return query
                .Select(item => new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Slot = item.Slot,
                    Type = item.Type,
                    Restrictions = item.Restrictions
                        .Where(r => (r.Specializations & Specializations.Warrior) != 0)
                        .Select(r => new RestrictionDto
                        {
                            Level = r.RestrictionLevel,
                            Reason = r.Reason,
                            Specs = r.Specializations
                        })
                        .ToList()
                })
                .AsSingleQuery()
                .AsAsyncEnumerable();
        }
    }
}
