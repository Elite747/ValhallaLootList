// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Xml.Serialization;

namespace ValhallaLootList.Client.Data.Containers
{
    [XmlRoot("EnumerationResults")]
    public class AzureContainerResponse
    {
        public string ContainerName { get; set; }

        [XmlArray("Blobs")]
        [XmlArrayItem("Blob")]
        public List<Blob> Blobs { get; set; }
    }
}
