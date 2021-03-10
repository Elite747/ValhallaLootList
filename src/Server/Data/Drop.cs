// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Drop : KeyedRow
    {
        public string? AwardedBy { get; set; }

        public DateTime AwardedAtUtc { get; set; }

        [Required]
        public uint ItemId { get; set; }

        public string? WinnerId { get; set; }

        [Required]
        public string EncounterKillRaidId { get; set; } = null!;

        [Required]
        public string EncounterKillEncounterId { get; set; } = null!;

        [Required]
        public virtual EncounterKill EncounterKill { get; set; } = null!;

        [Required]
        public virtual Item Item { get; set; } = null!;

        public virtual Character? Winner { get; set; }

        public virtual LootListEntry? WinningEntry { get; set; }

        public virtual ICollection<DropPass> Passes { get; set; } = new HashSet<DropPass>();
    }
}
