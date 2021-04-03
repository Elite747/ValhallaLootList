// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared
{
    public record ItemLinkContext
    {
        public uint? Id { get; set; }
        public Item? Item { get; set; }
        public bool Failed { get; set; }
        public bool Loading { get; set; }
    }
}
