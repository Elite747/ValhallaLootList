// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class BossKill
    {
        [Required]
        public string RaidId { get; set; } = null!;

        [Required]
        public string BossId { get; set; } = null!;

        [Required]
        public virtual Raid Raid { get; set; } = null!;

        [Required]
        public virtual Encounter Boss { get; set; } = null!;

        public virtual ICollection<Drop> Drops { get; set; } = new HashSet<Drop>();
    }
}
