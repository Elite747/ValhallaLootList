﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList
{
    public static class PlayerRaceExtensions
    {
        public static string GetDisplayName(this PlayerRace race)
        {
            return race switch
            {
                PlayerRace.Human => "Human",
                PlayerRace.Dwarf => "Dwarf",
                PlayerRace.NightElf => "Night Elf",
                PlayerRace.Gnome => "Gnome",
                PlayerRace.Draenei => "Draenei",
                _ => throw new ArgumentOutOfRangeException(nameof(race))
            };
        }

        public static bool IsValidRace(this PlayerRace value) => value switch
        {
            PlayerRace.Human => true,
            PlayerRace.Dwarf => true,
            PlayerRace.NightElf => true,
            PlayerRace.Gnome => true,
            PlayerRace.Draenei => true,
            _ => false
        };

        public static Classes GetClasses(this PlayerRace playerRace) => playerRace switch
        {
            PlayerRace.Human => Classes.Mage | Classes.Paladin | Classes.Priest | Classes.Rogue | Classes.Warlock | Classes.Warrior,
            PlayerRace.Dwarf => Classes.Hunter | Classes.Paladin | Classes.Priest | Classes.Rogue | Classes.Warrior,
            PlayerRace.NightElf => Classes.Druid | Classes.Hunter | Classes.Priest | Classes.Rogue | Classes.Warrior,
            PlayerRace.Gnome => Classes.Mage | Classes.Rogue | Classes.Warlock | Classes.Warrior,
            PlayerRace.Draenei => Classes.Hunter | Classes.Mage | Classes.Paladin | Classes.Priest | Classes.Shaman | Classes.Warrior,
            _ => throw new ArgumentOutOfRangeException(nameof(playerRace))
        };
    }
}
