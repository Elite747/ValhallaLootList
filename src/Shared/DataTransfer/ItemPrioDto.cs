// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class ItemPrioDto
    {
        public long CharacterId { get; set; }

        public string CharacterName { get; set; } = string.Empty;

        public int? Priority { get; set; }

        public string? Details { get; set; }

        public bool IsError { get; set; }
    }
}
