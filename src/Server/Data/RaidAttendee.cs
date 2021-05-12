// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class RaidAttendee
    {
        [Required]
        public long RaidId { get; set; }

        [Required]
        public long CharacterId { get; set; }

        [Required]
        public virtual Raid Raid { get; set; } = null!;

        [Required]
        public virtual Character Character { get; set; } = null!;

        public bool IgnoreAttendance { get; set; }

        [StringLength(256)]
        public string? IgnoreReason { get; set; }

        public long? RemovalId { get; set; }

        public virtual TeamRemoval? Removal { get; set; }
    }
}
