// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList
{
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
            _ => throw new ArgumentOutOfRangeException(nameof(classes))
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
            _ => throw new ArgumentOutOfRangeException(nameof(classes))
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
            _ => throw new ArgumentOutOfRangeException(nameof(classes))
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
            Classes.Warrior => Specializations.Warrior,
            Classes.Paladin => Specializations.Paladin,
            Classes.Hunter => Specializations.Hunter,
            Classes.Rogue => Specializations.Rogue,
            Classes.Priest => Specializations.Priest,
            Classes.Shaman => Specializations.Shaman,
            Classes.Mage => Specializations.Mage,
            Classes.Warlock => Specializations.Warlock,
            Classes.Druid => Specializations.Druid,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), "Parameter must be a single defined playable class."),
        };
    }
}
