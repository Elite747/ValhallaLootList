// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Items;

public class ItemProvider(ItemCache itemCache, WowheadClient wowheadClient)
{
    private readonly ItemCache _itemCache = itemCache;
    private readonly WowheadClient _wowheadClient = wowheadClient;

    public ValueTask<Item> GetItemAsync(uint id, CancellationToken cancellationToken = default)
    {
        return _itemCache.GetOrAddAsync(id, GetFromInteropAsync, cancellationToken);
    }

    private async Task<Item> GetFromInteropAsync(uint id, CancellationToken cancellationToken)
    {
        return await _wowheadClient.GetItemAsync(id, cancellationToken) switch
        {
            null => throw new Exception("Item does not exist"),
            WowheadItemResponse item => new Item(id, item.Name, item.Quality, item.Icon, item.Tooltip),
            WowheadErrorResponse error => throw new Exception(error.Error),
            _ => throw new Exception("Could not retrieve item info.")
        };
    }
}
