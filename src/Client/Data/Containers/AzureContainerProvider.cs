// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Containers;

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
        return _containerCache.GetOrAddAsync(container, _azureClient.GetContainerAsync, cancellationToken);
    }
}
