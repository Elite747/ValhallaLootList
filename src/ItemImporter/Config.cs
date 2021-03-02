// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.ItemImporter
{
    internal class Config
    {
        public string SeedInstancesPath { get; set; } = string.Empty;

        public string SeedItemsPath { get; set; } = string.Empty;

        public Dictionary<uint, uint[]> Tokens { get; } = new();
    }
}