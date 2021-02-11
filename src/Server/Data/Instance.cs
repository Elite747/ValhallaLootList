// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Instance : KeyedRow
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public byte Phase { get; set; }

        public virtual ICollection<Encounter> Encounters { get; set; } = new HashSet<Encounter>();
    }
}
