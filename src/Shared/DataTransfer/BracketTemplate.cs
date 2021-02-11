// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class BracketTemplate
    {
        public int HighestRank { get; set; }

        public int LowestRank { get; set; }

        public int ItemsPerRow { get; set; }

        public bool AllowOffSpec { get; set; }

        public bool AllowTypeDuplicates { get; set; }
    }
}
