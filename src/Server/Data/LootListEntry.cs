﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class LootListEntry : KeyedRow
    {
        [Required]
        public virtual CharacterLootList LootList { get; set; } = null!;

        public byte Rank { get; set; }

        public uint? ItemId { get; set; }

        public virtual Item? Item { get; set; }

        public bool Won { get; set; }

        public short PassCount { get; set; }
    }
}
