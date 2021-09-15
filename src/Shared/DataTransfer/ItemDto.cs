// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class ItemDto
    {
        public uint Id { get; set; }

        public uint QuestId { get; set; }

        public int MaxCount { get; set; }

        public string? Name { get; set; }

        public ItemType Type { get; set; }

        public InventorySlot Slot { get; set; }

        public ICollection<RestrictionDto> Restrictions { get; set; } = new List<RestrictionDto>();

        public uint? RewardFromId { get; set; }
    }
}
