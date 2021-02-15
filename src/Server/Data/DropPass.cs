// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class DropPass
    {
        [Required]
        public string CharacterId { get; set; } = null!;

        [Required]
        public string DropEncounterKillRaidId { get; set; } = null!;

        [Required]
        public string DropEncounterKillEncounterId { get; set; } = null!;

        [Required]
        public uint DropItemId { get; set; }

        [Required]
        public virtual Drop Drop { get; set; } = null!;

        [Required]
        public virtual Character Character { get; set; } = null!;

        public int RelativePriority { get; set; }
    }
}
