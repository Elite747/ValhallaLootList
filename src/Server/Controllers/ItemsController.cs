// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
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

        public async Task<IEnumerable<ItemDto>> Get(byte phase, bool includeTokens = false)
        {
            var itemQuery = _context.Items.AsNoTracking().Where(item => item.Phase == phase);
            var restrictionQuery = _context.ItemRestrictions.AsNoTracking().Where(r => r.Item.Phase == phase);

            if (!includeTokens)
            {
                itemQuery = itemQuery.Where(item => item.Slot != InventorySlot.Unknown);
                restrictionQuery = restrictionQuery.Where(r => r.Item.Slot != InventorySlot.Unknown);
            }

            var items = await itemQuery
                .Select(item => new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Slot = item.Slot,
                    Type = item.Type,
                })
                .ToDictionaryAsync(item => item.Id);

            await foreach (var restriction in restrictionQuery
                .OrderBy(r => r.ItemId)
                .ThenBy(r => r.RestrictionLevel)
                .Select(r => new
                {
                    r.ItemId,
                    r.Reason,
                    r.RestrictionLevel,
                    r.Specializations
                })
                .AsAsyncEnumerable())
            {
                if (items.TryGetValue(restriction.ItemId, out var item))
                {
                    item.Restrictions.Add(new RestrictionDto
                    {
                        Level = restriction.RestrictionLevel,
                        Reason = restriction.Reason,
                        Specs = restriction.Specializations
                    });
                }
            }

            return items.Values;
        }

        [HttpGet("{itemId:int}/Restrictions")]
        public IAsyncEnumerable<RestrictionDto> GetRestrictions(uint itemId)
        {
            return _context.ItemRestrictions
                .AsNoTracking()
                .Where(r => r.ItemId == itemId)
                .OrderBy(r => r.RestrictionLevel)
                .Select(r => new RestrictionDto
                {
                    Level = r.RestrictionLevel,
                    Reason = r.Reason,
                    Specs = r.Specializations
                })
                .AsAsyncEnumerable();
        }
    }
}
