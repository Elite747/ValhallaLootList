// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValhallaLootList.Client.Data.Items
{
    public class ItemProvider
    {
        private readonly ItemCache _itemCache;
        private readonly WowheadInterop _wowheadInterop;
        private readonly WowheadClient _wowheadClient;
        private (int type, int env, int locale)? _config;

        public ItemProvider(ItemCache itemCache, WowheadInterop wowheadInterop, WowheadClient wowheadClient)
        {
            _itemCache = itemCache;
            _wowheadInterop = wowheadInterop;
            _wowheadClient = wowheadClient;
        }

        public ValueTask<Item> GetItemAsync(uint id, CancellationToken cancellationToken = default)
        {
            return _itemCache.GetOrAddAsync(id, GetFromInteropAsync, cancellationToken);
        }

        private async Task<Item> GetFromInteropAsync(uint id, CancellationToken cancellationToken)
        {
            _config ??= await GetConfigurationAsync(cancellationToken);

            var (type, env, locale) = _config.Value;

            var response = await _wowheadInterop.GetEntityAsync(type, id.ToString(), env, locale, cancellationToken);

            if (response is WowheadItemResponse item)
            {
                return new Item(id, item.Name, item.Quality, item.Icon, item.Tooltip);
            }

            response = await _wowheadClient.GetItemAsync(id, cancellationToken);

            if (response is null)
            {
                throw new Exception("Item does not exist.");
            }

            await _wowheadInterop.RegisterEntityAsync(type, id.ToString(), env, locale, response, cancellationToken);

            if (response is WowheadItemResponse item2)
            {
                return new Item(id, item2.Name, item2.Quality, item2.Icon, item2.Tooltip);
            }

            throw new Exception((response as WowheadErrorResponse)?.Error ?? "Could not retrieve item info.");
        }

        private async Task<(int type, int env, int locale)> GetConfigurationAsync(CancellationToken cancellationToken)
        {
            return (
                await _wowheadInterop.GetTypeIdFromTypeStringAsync("item", cancellationToken),
                await _wowheadInterop.GetDataEnvFromTermAsync("live", cancellationToken), // TODO: change this to 'burningCrusade' when wowhead supports it.
                await _wowheadInterop.GetLocaleFromDomainAsync(_wowheadClient.GetDomain(), cancellationToken)
                );
        }
    }
}
