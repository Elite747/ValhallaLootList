// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Encounter : KeyedRow
    {
        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string InstanceId { get; set; } = null!;

        public sbyte Index { get; set; }

        [Required]
        public virtual Instance Instance { get; set; } = null!;

        public virtual ICollection<Item> Items { get; set; } = new HashSet<Item>();
    }
}
