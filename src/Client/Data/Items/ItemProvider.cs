// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace ValhallaLootList.Client.Data.Items
{
    public class ItemProvider
    {
        private readonly ItemCache _itemCache;
        private readonly WowheadInterop _wowheadInterop;
        private readonly WowheadClient _wowheadClient;
        private readonly ConcurrentDictionary<uint, Task<Item>> _itemOperations;
        private (int type, int env, int locale)? _config;

        public ItemProvider(ItemCache itemCache, WowheadInterop wowheadInterop, WowheadClient wowheadClient)
        {
            _itemCache = itemCache;
            _wowheadInterop = wowheadInterop;
            _wowheadClient = wowheadClient;
            _itemOperations = new();
        }

        public ValueTask<Item> GetItemAsync(uint id, CancellationToken cancellationToken = default)
        {
            var item = _itemCache.GetByKey(id);

            if (item is not null)
            {
                return new(item);
            }

            return new(_itemOperations.GetOrAdd(id, id => GetFromInteropAsync(id, cancellationToken)));
        }

        private async Task<Item> GetFromInteropAsync(uint id, CancellationToken cancellationToken)
        {
            try
            {
                var cacheItem = _itemCache.GetByKey(id);

                if (cacheItem is not null)
                {
                    return cacheItem;
                }

                _config ??= await GetConfigurationAsync(cancellationToken);

                var (type, env, locale) = _config.Value;

                var response = await _wowheadInterop.GetEntityAsync(type, id.ToString(), env, locale, cancellationToken);

                if (response is WowheadItemResponse item)
                {
                    return new Item(id, item.Name, item.Quality, item.Icon, item.Tooltip);
                }
                return await DownloadAndCacheAsync(id, type, env, locale, cancellationToken);
            }
            finally
            {
                _itemOperations.TryRemove(id, out _);
            }
        }

        private async Task<(int type, int env, int locale)> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            return (
                await _wowheadInterop.GetTypeIdFromTypeStringAsync("item", cancellationToken),
                await _wowheadInterop.GetDataEnvFromTermAsync("live", cancellationToken), // TODO: change this to 'burningCrusade' when wowhead supports it.
                await _wowheadInterop.GetLocaleFromDomainAsync(_wowheadClient.GetDomain(), cancellationToken)
                );
        }

        private async Task<Item> DownloadAndCacheAsync(uint id, int type, int env, int locale, CancellationToken cancellationToken)
        {
            var response = await _wowheadClient.GetItemAsync(id, cancellationToken);

            if (response is null)
            {
                throw new Exception("Item does not exist.");
            }

            await _wowheadInterop.RegisterEntityAsync(type, id.ToString(), env, locale, response, cancellationToken);

            if (response is WowheadItemResponse itemResponse)
            {
                var item = new Item(id, itemResponse.Name, itemResponse.Quality, itemResponse.Icon, itemResponse.Tooltip);
                _itemCache.UpdateCache(item);
                return item;
            }

            throw new Exception((response as WowheadErrorResponse)?.Error ?? "Could not retrieve item info.");
        }
    }
}
