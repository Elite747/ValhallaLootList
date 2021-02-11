// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class ItemRestriction : KeyedRow
    {
		public uint ItemId { get; set; }

		public virtual Item Item { get; set; } = null!;

		public Specializations Specializations { get; set; }

		public ItemRestrictionLevel RestrictionLevel { get; set; }

		[Required, StringLength(256, MinimumLength = 1)]
		public string Reason { get; set; } = null!;

		public bool Automated { get; set; }
    }
}
