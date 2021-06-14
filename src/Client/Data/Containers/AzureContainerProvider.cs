// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValhallaLootList.Client.Data.Containers
{
    public class AzureContainerProvider
    {

        private readonly AzureContainerCache _containerCache;
        private readonly AzureClient _azureClient;

        public AzureContainerProvider(AzureContainerCache containerCache, AzureClient azureClient)
        {
            _containerCache = containerCache;
            _azureClient = azureClient;
        }

        public ValueTask<AzureContainerResponse> GetContainerAsync(string container, CancellationToken cancellationToken = default)
        {
            return _containerCache.GetOrAddAsync(container, GetFromInteropAsync, cancellationToken);
        }

        private async Task<AzureContainerResponse> GetFromInteropAsync(string container, CancellationToken cancellationToken)
        {
            var response = await _azureClient.GetContainerAsync(container, cancellationToken);

            if (response is null)
            {
                throw new Exception("Container does not exist.");
            }

            if (response is AzureContainerResponse azureContainerResponse)
            {
                return azureContainerResponse;
            }

            throw new Exception("Could not retrieve container info.");
        }
    }
}
