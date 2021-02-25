// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data.Instances
{
    public class InstanceProvider
    {
        private readonly ApiClient _apiClient;
        private readonly InstanceCache _instanceCache;

        public InstanceProvider(ApiClient apiClient, InstanceCache instanceCache)
        {
            _apiClient = apiClient;
            _instanceCache = instanceCache;
        }

        public bool RequiresLoading => _instanceCache.IsEmpty;

        public ValueTask EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            if (_instanceCache.IsEmpty)
            {
                return new(DownloadAsync(cancellationToken));
            }
            return default;
        }

        public IEnumerable<InstanceDto> GetCached() => _instanceCache.EnumerateCached();

        public InstanceDto? FindCached(Func<InstanceDto, bool> predicate)
        {
            foreach (var instance in _instanceCache.EnumerateCached())
            {
                if (predicate(instance))
                {
                    return instance;
                }
            }

            return null;
        }

        public InstanceDto? FindCached(string id) => _instanceCache.GetByKey(id);

        private async Task DownloadAsync(CancellationToken cancellationToken)
        {
            var instances = await _apiClient.GetAsync<List<InstanceDto>>("api/v1/instances", cancellationToken);

            if (instances is not null)
            {
                _instanceCache.UpdateCache(instances);
            }
        }
    }
}
