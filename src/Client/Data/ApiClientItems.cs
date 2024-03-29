﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientItems(ApiClient client)
{
    public ApiClient Client { get; } = client;

    public IApiClientOperation<List<ItemDto>> Get(byte phase, byte size, bool includeTokens = false)
    {
        return Client.CreateRequest<List<ItemDto>>(HttpMethod.Get, $"api/v1/items?phase={phase}&size={size}&includeTokens={includeTokens}").CacheFor(TimeSpan.FromHours(6));
    }

    public IApiClientOperation<List<RestrictionDto>> GetRestrictions(uint itemId)
    {
        return Client.CreateRequest<List<RestrictionDto>>(HttpMethod.Get, $"api/v1/items/{itemId}/restrictions");
    }
}
