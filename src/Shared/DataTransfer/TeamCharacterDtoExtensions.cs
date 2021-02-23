// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;

namespace ValhallaLootList.DataTransfer
{
    public static class TeamCharacterDtoExtensions
    {
        public static IOrderedEnumerable<TeamCharacterDto> OrderByRoleThenClassThenName(this IEnumerable<TeamCharacterDto> characters)
        {
            return characters
                .OrderBy(c => GetRoleSortedIndex(c.CurrentPhaseMainspec))
                .ThenBy(c => c.Class == Classes.None ? null : c.Class.ToString())
                .ThenBy(c => c.CurrentPhaseMainspec?.ToString())
                .ThenBy(c => c.Name);
        }

        private static int GetRoleSortedIndex(Specializations? spec)
        {
            if (spec.HasValue)
            {
                if ((spec.Value & Specializations.Tank) != 0)
                {
                    return 1;
                }

                if ((spec.Value & Specializations.Healer) != 0)
                {
                    return 2;
                }

                return 3;
            }
            return int.MaxValue;
        }
    }
}
