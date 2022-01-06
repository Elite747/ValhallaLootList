// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data.Items;

public class ItemCache : Cache<Item, uint>
{
    public ItemCache(IMemoryCache memoryCache) : base(memoryCache)
    {
    }

    protected override uint GetKey(Item item) => item.Id;

    protected override MemoryCacheEntryOptions CreateCacheEntryOptions(Item item) => base.CreateCacheEntryOptions(item).SetAbsoluteExpiration(TimeSpan.FromHours(1));
}
