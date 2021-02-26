// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IAsyncEnumerable<ItemDto> Get(byte? phase, Specializations? spec, bool includeTokens = false)
        {
            IQueryable<Item> results = _context.Items.AsNoTracking();

            if (phase.HasValue)
            {
                results = results.Where(item => (item.RewardFromId != null ? item.RewardFrom!.Encounter!.Instance.Phase : item.Encounter!.Instance!.Phase) == phase.Value); // TODO: Simplify phase query
            }

            if (!includeTokens)
            {
                results = results.Where(item => item.Slot != InventorySlot.Unknown);
            }

            if (spec.HasValue && spec != Specializations.All)
            {
                var paramExp = Expression.Parameter(typeof(ItemRestriction));
                var specPropertyExp = Expression.Convert(Expression.Property(paramExp, nameof(ItemRestriction.Specializations)), typeof(int));

                Expression body = Expression.Equal(Expression.Property(paramExp, nameof(ItemRestriction.RestrictionLevel)), Expression.Constant(ItemRestrictionLevel.Unequippable));

                Expression? specCheck = null;

                foreach (var singleSpec in spec.Value.Split())
                {
                    var singleSpecExp = Expression.Constant((int)singleSpec);
                    var thisCheck = Expression.Equal(Expression.And(specPropertyExp, singleSpecExp), singleSpecExp);
                    specCheck = specCheck is null ? thisCheck : Expression.OrElse(specCheck, thisCheck);
                }

                if (specCheck is not null)
                {
                    body = Expression.AndAlso(body, specCheck);
                }

                Expression.Lambda<Func<ItemRestriction, bool>>(body, paramExp);

                results = results.Where(item => !item.Restrictions.AsQueryable().Any(Expression.Lambda<Func<ItemRestriction, bool>>(body, paramExp)));
            }

            return results
                .Select(item => new ItemDto
                {
                    Id = item.Id,
                    Name = item.Name,
                    Slot = item.Slot,
                    Type = item.Type,
                    Restrictions = item.Restrictions
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
