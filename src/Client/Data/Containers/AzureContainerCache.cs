// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data.Containers
{
    public class AzureContainerCache : Cache<AzureContainerResponse, string>
    {
        public AzureContainerCache(IMemoryCache memoryCache) : base(memoryCache)
        {
        }

        protected override string GetKey(AzureContainerResponse response) => response.ContainerName;

        protected override MemoryCacheEntryOptions CreateCacheEntryOptions(AzureContainerResponse response) => base.CreateCacheEntryOptions(response).SetAbsoluteExpiration(TimeSpan.FromHours(1));
    }
}
