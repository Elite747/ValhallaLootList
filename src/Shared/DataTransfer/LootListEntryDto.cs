// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class LootListEntryDto
    {
        private List<PriorityBonusDto>? _bonuses;

        public long Id { get; set; }

        public int Rank { get; set; }

        public uint? ItemId { get; set; }

        public uint? RewardFromId { get; set; }

        public string? ItemName { get; set; }

        public string? Justification { get; set; }

        public bool AutoPass { get; set; }

        public bool Won { get; set; }

        public int Bracket { get; set; }

        public bool BracketAllowsOffspec { get; set; }

        public bool BracketAllowsTypeDuplicates { get; set; }

        public List<PriorityBonusDto> Bonuses
        {
            get => _bonuses ??= new();
            set => _bonuses = value;
        }
    }
}
