// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Containers;

public class AzureContainerResponse(string containerName, List<Blob> blobs)
{
    public string ContainerName { get; } = containerName;

    public List<Blob> Blobs { get; } = blobs;
}
