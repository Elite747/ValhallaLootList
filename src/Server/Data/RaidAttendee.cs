// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class RaidAttendee
    {
        [Required]
        public string RaidId { get; set; } = null!;

        [Required]
        public string CharacterId { get; set; } = null!;

        [Required]
        public virtual Raid Raid { get; set; } = null!;

        [Required]
        public virtual Character Character { get; set; } = null!;

        public bool IgnoreAttendance { get; set; }

        public string? IgnoreReason { get; set; }

        public bool UsingOffspec { get; set; }
    }
}
