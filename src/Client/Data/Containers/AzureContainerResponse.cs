// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.Client.Data.Containers
{
    public class AzureContainerResponse
    {
        public AzureContainerResponse(string containerName, List<Blob> blobs)
        {
            ContainerName = containerName;
            Blobs = blobs;
        }

        public string ContainerName { get; }

        public List<Blob> Blobs { get; }
    }
}
