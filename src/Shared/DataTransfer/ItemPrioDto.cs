// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class ItemPrioDto
    {
        private List<PriorityBonusDto>? _bonuses;

        public long CharacterId { get; set; }

        public string CharacterName { get; set; } = string.Empty;

        public bool Locked { get; set; }

        public bool Approved { get; set; }

        public int Rank { get; set; }

        public List<PriorityBonusDto> Bonuses
        {
            get => _bonuses ??= new();
            set => _bonuses = value;
        }
    }
}
