// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientItems
    {
        public ApiClientItems(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<List<ItemDto>> Get(byte phase, bool includeTokens = false)
        {
            return Client.CreateRequest<List<ItemDto>>(HttpMethod.Get, $"api/v1/items?phase={phase}&includeTokens={includeTokens}").CacheFor(TimeSpan.FromHours(1));
        }

        public IApiClientOperation<List<RestrictionDto>> GetRestrictions(uint itemId)
        {
            return Client.CreateRequest<List<RestrictionDto>>(HttpMethod.Get, $"api/v1/items/{itemId}/restrictions");
        }
    }
}
