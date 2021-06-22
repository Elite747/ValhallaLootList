// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.IO;

namespace ValhallaLootList.Client.Data.Containers
{
    public class Blob
    {
        public Blob(string name, string url)
        {
            Name = name;
            Url = url;
        }

        public string Name { get; }

        public string Simple => Path.GetFileNameWithoutExtension(Name);

        public string Url { get; }
    }
}
