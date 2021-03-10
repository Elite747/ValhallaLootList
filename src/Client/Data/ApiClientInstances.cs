// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientInstances
    {
        public ApiClientInstances(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IEnumerable<InstanceDto>> GetAll()
        {
            return Client.CreateRequest<IEnumerable<InstanceDto>>(HttpMethod.Get, "api/v1/instances").CacheFor(TimeSpan.FromHours(2));
        }
    }
}
