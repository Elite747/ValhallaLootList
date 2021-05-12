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
            return characters.OrderBy(c => GetRoleAndClassSortedIndex(c.LootLists, phase)).ThenBy(c => c.Character.Name);
        }

        private static int GetRoleAndClassSortedIndex(IEnumerable<MemberLootListDto> lootLists, byte phase)
        {
            foreach (var lootList in lootLists)
            {
                if (lootList.Phase == phase)
                {
                    return lootList.MainSpec.GetSortingIndex();
                }
            }

            return int.MaxValue;
        }
    }
}
