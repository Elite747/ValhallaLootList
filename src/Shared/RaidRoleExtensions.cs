// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

public static class RaidRoleExtensions
{
    public static string GetDisplayName(this RaidRole role, bool singular = false)
    {
        return role switch
        {
            RaidRole.Tank => singular ? "Tank" : "Tanks",
            RaidRole.Healer => singular ? "Healer" : "Healers",
            RaidRole.Dps => "DPS",
            RaidRole.Unknown => "Unknown Role",
            _ => throw new ArgumentOutOfRangeException(nameof(role))
        };
    }
}
