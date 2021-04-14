// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data
{
    public class MonthDonations
    {
        public long CharacterId { get; set; }

        public int Month { get; set; }

        public int Year { get; set; }

        public long Donated { get; set; }
    }
}
