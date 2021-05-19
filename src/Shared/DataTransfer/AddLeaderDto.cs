// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class AddLeaderDto
    {
        public long MemberId { get; set; }

        public bool RaidLeader { get; set; }

        public bool LootMaster { get; set; }

        public bool Recruiter { get; set; }
    }
}
