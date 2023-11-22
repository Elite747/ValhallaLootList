// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics.CodeAnalysis;

namespace ValhallaLootList;

public static class PlayerRaceExtensions
{
    private static readonly PlayerRace[] _allRaces = [PlayerRace.Human, PlayerRace.Dwarf, PlayerRace.NightElf, PlayerRace.Gnome, PlayerRace.Draenei];

    private static readonly Dictionary<PlayerRace, IEnumerable<Classes>> _classesByRace = new()
    {
        [PlayerRace.Human] = new[] { Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warlock, Classes.Warrior, Classes.DeathKnight },
        [PlayerRace.Dwarf] = new[] { Classes.Hunter, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warrior, Classes.DeathKnight },
        [PlayerRace.NightElf] = new[] { Classes.Druid, Classes.Hunter, Classes.Priest, Classes.Rogue, Classes.Warrior, Classes.DeathKnight },
        [PlayerRace.Gnome] = new[] { Classes.Mage, Classes.Rogue, Classes.Warlock, Classes.Warrior, Classes.DeathKnight },
        [PlayerRace.Draenei] = new[] { Classes.Hunter, Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Shaman, Classes.Warrior, Classes.DeathKnight }
    };

    public static IEnumerable<PlayerRace> GetAll()
    {
        return _allRaces;
    }

    public static IEnumerable<Classes> GetClasses(this PlayerRace race)
    {
        if (_classesByRace.TryGetValue(race, out var classes))
        {
            return classes;
        }
        throw new ArgumentOutOfRangeException(nameof(race));
    }

    public static bool TryGetClasses(this PlayerRace race, [NotNullWhen(true)] out IEnumerable<Classes>? classes)
    {
        return _classesByRace.TryGetValue(race, out classes);
    }

    public static string GetDisplayName(this PlayerRace race)
    {
        return race switch
        {
            PlayerRace.Human => "Human",
            PlayerRace.Dwarf => "Dwarf",
            PlayerRace.NightElf => "Night Elf",
            PlayerRace.Gnome => "Gnome",
            PlayerRace.Draenei => "Draenei",
            _ => "Unknown"
        };
    }

    public static bool IsValidRace(this PlayerRace value)
    {
        return value switch
        {
            PlayerRace.Human => true,
            PlayerRace.Dwarf => true,
            PlayerRace.NightElf => true,
            PlayerRace.Gnome => true,
            PlayerRace.Draenei => true,
            _ => false
        };
    }
}
