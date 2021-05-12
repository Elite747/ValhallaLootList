// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList
{
    public static class AppRoles
    {
        public const string Member = nameof(Member);

        public const string Administrator = nameof(Administrator);

        public const string RaidLeader = nameof(RaidLeader);

        public const string LootMaster = nameof(LootMaster);

        public static IEnumerable<string> All
        {
            get
            {
                yield return Member;
                yield return Administrator;
                yield return RaidLeader;
                yield return LootMaster;
            }
        }
    }
}
