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

        public IApiClientOperation<IList<ItemDto>> Get(byte phase)
        {
            return Client.CreateRequest<IList<ItemDto>>(HttpMethod.Get, $"api/v1/items?phase={phase}").CacheFor(TimeSpan.FromHours(1));
        }

        public IApiClientOperation<IList<RestrictionDto>> GetRestrictions(uint itemId)
        {
            return Client.CreateRequest<IList<RestrictionDto>>(HttpMethod.Get, $"api/v1/items/{itemId}/restrictions");
        }
    }
}
