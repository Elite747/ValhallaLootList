// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.ItemImporter
{
    internal class Config
    {
        public Dictionary<string, InstanceConfig> Instances { get; set; }
        public Dictionary<uint, uint[]> Tokens { get; set; }
        public Dictionary<string, string> BossNameOverrides { get; set; }
        public Dictionary<uint, string> BossOverrides { get; set; }
    }
}