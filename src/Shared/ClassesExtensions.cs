// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics.CodeAnalysis;

namespace ValhallaLootList;

public static class ClassesExtensions
{
    private static readonly Classes[] _allClasses =
    [
        Classes.DeathKnight,
        Classes.Druid,
        Classes.Hunter,
        Classes.Mage,
        Classes.Paladin,
        Classes.Priest,
        Classes.Rogue,
        Classes.Shaman,
        Classes.Warlock,
        Classes.Warrior
    ];

    public static readonly Dictionary<Classes, IEnumerable<PlayerRace>> _raceLookup = new()
    {
        [Classes.Druid] = new[] { PlayerRace.NightElf },
        [Classes.Hunter] = new[] { PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Draenei },
        [Classes.Mage] = new[] { PlayerRace.Human, PlayerRace.Gnome, PlayerRace.Draenei },
        [Classes.Paladin] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.Draenei },
        [Classes.Priest] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Draenei },
        [Classes.Rogue] = new[] { PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Gnome },
        [Classes.Shaman] = new[] { PlayerRace.Draenei },
        [Classes.Warlock] = new[] { PlayerRace.Human, PlayerRace.Gnome },
        [Classes.Warrior] = PlayerRaceExtensions.GetAll(),
        [Classes.DeathKnight] = PlayerRaceExtensions.GetAll()
    };

    public static IEnumerable<Classes> GetAll()
    {
        return _allClasses;
    }

    public static IEnumerable<PlayerRace> GetRaces(this Classes playerClass)
    {
        if (_raceLookup.TryGetValue(playerClass, out var races))
        {
            return races;
        }
        throw new ArgumentOutOfRangeException(nameof(playerClass));
    }

    public static bool TryGetRaces(this Classes playerClass, [NotNullWhen(true)] out IEnumerable<PlayerRace>? races)
    {
        return _raceLookup.TryGetValue(playerClass, out races);
    }

    public static string GetDisplayName(this Classes classes)
    {
        return classes switch
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
            Classes.DeathKnight => "Death Knight",
            _ => "Unknown"
        };
    }

    public static string GetLowercaseName(this Classes classes)
    {
        return classes switch
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
            Classes.DeathKnight => "deathknight",
            _ => "unknown"
        };
    }

    public static string GetClassColor(this Classes classes)
    {
        return classes switch
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
            Classes.DeathKnight => "#C41E3A",
            _ => "#000000"
        };
    }

    public static bool IsSingleClass(this Classes playerClass)
    {
        return playerClass switch
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
            Classes.DeathKnight => true,
            _ => false
        };
    }

    public static Specializations ToSpecializations(this Classes playerClass)
    {
        return playerClass switch
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
            Classes.DeathKnight => SpecializationGroups.DeathKnight,
            _ => throw new ArgumentOutOfRangeException(nameof(playerClass), "Parameter must be a single defined playable class."),
        };
    }

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
            Classes.DeathKnight => 0,
            _ => int.MaxValue
        };
    }
}
