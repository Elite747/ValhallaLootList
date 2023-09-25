// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

[Flags]
public enum Classes
{
    None = 0,
    Warrior = 1 << 0,
    Paladin = 1 << 1,
    Hunter = 1 << 2,
    Rogue = 1 << 3,
    Priest = 1 << 4,
    DeathKnight = 1 << 5,
    Shaman = 1 << 6,
    Mage = 1 << 7,
    Warlock = 1 << 8,
    Druid = 1 << 10
}
