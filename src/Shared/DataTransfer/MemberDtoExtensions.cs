// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;

namespace ValhallaLootList.DataTransfer
{
    public static class MemberDtoExtensions
    {
        public static IOrderedEnumerable<MemberDto> OrderByRoleThenClassThenName(this IEnumerable<MemberDto> characters, byte phase)
        {
            return characters
                .OrderBy(c => GetRoleSortedIndex(c.LootLists, phase))
                .ThenBy(c => GetClassSortedIndex(c.Character.Class))
                .ThenBy(c => c.Character.Name);
        }

        private static int GetRoleSortedIndex(IEnumerable<MemberLootListDto> lootLists, byte phase)
        {
            foreach (var lootList in lootLists)
            {
                if (lootList.Phase == phase)
                {
                    if ((lootList.MainSpec & Specializations.Tank) != 0)
                    {
                        return 1;
                    }

                    if ((lootList.MainSpec & Specializations.Healer) != 0)
                    {
                        return 2;
                    }

                    return 3;
                }
            }

            return int.MaxValue;
        }

        private static int GetClassSortedIndex(Classes classes)
        {
            return classes switch
            {
                Classes.Warrior => 9,
                Classes.Paladin => 4,
                Classes.Hunter => 2,
                Classes.Rogue => 6,
                Classes.Priest => 5,
                Classes.Shaman => 7,
                Classes.Mage => 3,
                Classes.Warlock => 8,
                Classes.Druid => 1,
                _ => int.MaxValue
            };
        }
    }
}
