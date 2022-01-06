// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Items;

public class ItemProvider
{
    private readonly ItemCache _itemCache;
    private readonly WowheadClient _wowheadClient;

    public ItemProvider(ItemCache itemCache, WowheadClient wowheadClient)
    {
        _itemCache = itemCache;
        _wowheadClient = wowheadClient;
    }

    public ValueTask<Item> GetItemAsync(uint id, CancellationToken cancellationToken = default)
    {
        return _itemCache.GetOrAddAsync(id, GetFromInteropAsync, cancellationToken);
    }

    private async Task<Item> GetFromInteropAsync(uint id, CancellationToken cancellationToken)
    {
        var response = await _wowheadClient.GetItemAsync(id, cancellationToken);

        if (response is null)
        {
            throw new Exception("Item does not exist.");
        }

        if (response is WowheadItemResponse item2)
        {
            return new Item(id, item2.Name, item2.Quality, item2.Icon, item2.Tooltip);
        }

        throw new Exception((response as WowheadErrorResponse)?.Error ?? "Could not retrieve item info.");
    }
}
