// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class LootListEntryUpdateDto
    {
        public long EntryId { get; set; }

        public uint? ItemId { get; set; }

        public string? EntryJustification { get; set; }

        public long? SwapEntryId { get; set; }

        public uint? SwapItemId { get; set; }

        public string? SwapEntryJustification { get; set; }
    }
}
