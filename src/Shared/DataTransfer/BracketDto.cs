// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class BracketDto
    {
        public int MaxRank { get; set; }

        public int MinRank { get; set; }

        public int MaxItems { get; set; }

        public bool AllowOffSpec { get; set; }

        public bool AllowTypeDuplicates { get; set; }
    }
}
