// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class EncounterDto
    {
        private IList<uint>? _items;

        public string? Id { get; set; }

        public string? Name { get; set; }

        public IList<uint> Items
        {
            get => _items ??= new List<uint>();
            set => _items = value;
        }
    }
}
