// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class EncounterKill
    {
        public DateTime KilledAtUtc { get; set; }

        [Required]
        public string RaidId { get; set; } = null!;

        [Required]
        public string EncounterId { get; set; } = null!;

        [Required]
        public virtual Raid Raid { get; set; } = null!;

        [Required]
        public virtual Encounter Encounter { get; set; } = null!;

        public virtual ICollection<Drop> Drops { get; set; } = new HashSet<Drop>();

        public virtual ICollection<CharacterEncounterKill> Characters { get; set; } = new HashSet<CharacterEncounterKill>();
    }
}
