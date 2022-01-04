// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

public static class ClassesExtensions
{
    public static string GetDisplayName(this Classes classes) => classes switch
    {
        Classes.Warrior => "Warrior",
        Classes.Paladin => "Paladin",
        Classes.Hunter => "Hunter",
        Classes.Rogue => "Rogue",
        Classes.Priest => "Priest",
        Classes.Shaman => "Shaman",
        Classes.Mage => "Mage",
        Classes.Warlock => "Warlock",
        Classes.Druid => "Druid",
        _ => "Unknown"
    };

    public static string GetLowercaseName(this Classes classes) => classes switch
    {
        Classes.Warrior => "warrior",
        Classes.Paladin => "paladin",
        Classes.Hunter => "hunter",
        Classes.Rogue => "rogue",
        Classes.Priest => "priest",
        Classes.Shaman => "shaman",
        Classes.Mage => "mage",
        Classes.Warlock => "warlock",
        Classes.Druid => "druid",
        _ => "unknown"
    };

    public static string GetClassColor(this Classes classes) => classes switch
    {
        Classes.Warrior => "#C69B6D",
        Classes.Paladin => "#F48CBA",
        Classes.Hunter => "#AAD372",
        Classes.Rogue => "#FFF468",
        Classes.Priest => "#FFFFFF",
        Classes.Shaman => "#0070DD",
        Classes.Mage => "#3FC7EB",
        Classes.Warlock => "#8788EE",
        Classes.Druid => "#FF7C0A",
        _ => "#000000"
    };

    public static bool IsSingleClass(this Classes playerClass) => playerClass switch
    {
        Classes.Warrior => true,
        Classes.Paladin => true,
        Classes.Hunter => true,
        Classes.Rogue => true,
        Classes.Priest => true,
        Classes.Shaman => true,
        Classes.Mage => true,
        Classes.Warlock => true,
        Classes.Druid => true,
        _ => false
    };

    public static Specializations ToSpecializations(this Classes playerClass) => playerClass switch
    {
        Classes.Warrior => SpecializationGroups.Warrior,
        Classes.Paladin => SpecializationGroups.Paladin,
        Classes.Hunter => SpecializationGroups.Hunter,
        Classes.Rogue => SpecializationGroups.Rogue,
        Classes.Priest => SpecializationGroups.Priest,
        Classes.Shaman => SpecializationGroups.Shaman,
        Classes.Mage => SpecializationGroups.Mage,
        Classes.Warlock => SpecializationGroups.Warlock,
        Classes.Druid => SpecializationGroups.Druid,
        _ => throw new ArgumentOutOfRangeException(nameof(playerClass), "Parameter must be a single defined playable class."),
    };

    public static int GetSortingIndex(this Classes classes)
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
