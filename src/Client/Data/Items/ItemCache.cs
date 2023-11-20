// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data.Items;

public class ItemCache(IMemoryCache memoryCache) : Cache<Item, uint>(memoryCache)
{
    protected override uint GetKey(Item item)
    {
        return item.Id;
    }

    protected override MemoryCacheEntryOptions CreateCacheEntryOptions(Item item)
    {
        return base.CreateCacheEntryOptions(item).SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }
}
