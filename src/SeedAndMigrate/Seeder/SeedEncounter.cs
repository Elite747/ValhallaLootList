// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.SeedAndMigrate.Seeder
{
    public class SeedEncounter
    {
        private List<uint>? _items;

        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public sbyte Index { get; set; }

        public List<uint> Items
        {
            get => _items ??= new();
            set => _items = value;
        }
    }
}
