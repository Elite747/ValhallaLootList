// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Drop
    {
        public string? AwardedBy { get; set; }

        public DateTime AwardedAtUtc { get; set; }

        [Required]
        public string BossKillId { get; set; } = null!;

        [Required]
        public uint ItemId { get; set; }

        [Required]
        public virtual BossKill BossKill { get; set; } = null!;

        [Required]
        public virtual Item Item { get; set; } = null!;

        public virtual Character? Winner { get; set; } = null!;

        public virtual ICollection<DropPass> Passes { get; set; } = new HashSet<DropPass>();
    }
}
