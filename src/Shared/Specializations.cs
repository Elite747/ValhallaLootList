// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList
{
    [Flags]
    public enum Specializations
    {
        None = 0,
        BalanceDruid = 1 << 0,
        BearDruid = 1 << 1,
        CatDruid = 1 << 2,
        RestoDruid = 1 << 3,
        Hunter = 1 << 4,
        Mage = 1 << 5,
        HolyPaladin = 1 << 6,
        ProtPaladin = 1 << 7,
        RetPaladin = 1 << 8,
        HealerPriest = 1 << 9,
        ShadowPriest = 1 << 10,
        Rogue = 1 << 11, // split rogue by weapon type?
        EleShaman = 1 << 12,
        EnhanceShaman = 1 << 13,
        RestoShaman = 1 << 14,
        Warlock = 1 << 15,
        ProtWarrior = 1 << 16,
        ArmsWarrior = 1 << 17,
        FuryWarrior = 1 << 18,

        Tank = BearDruid | ProtPaladin | ProtWarrior,
        Healer = RestoDruid | RestoShaman | HolyPaladin | HealerPriest,
        MeleeDps = CatDruid | Rogue | EnhanceShaman | RetPaladin | ArmsWarrior | FuryWarrior,
        CasterDps = BalanceDruid | Mage | ShadowPriest | EleShaman | Warlock,
        PhysicalDps = MeleeDps | Hunter,
        PhysicalCaster = ProtPaladin | EnhanceShaman | RetPaladin,

        Druid = BalanceDruid | BearDruid | CatDruid | RestoDruid,
        Paladin = HolyPaladin | ProtPaladin | RetPaladin,
        Priest = HealerPriest | ShadowPriest,
        Shaman = EleShaman | EnhanceShaman | RestoShaman,
        Warrior = ProtWarrior | ArmsWarrior | FuryWarrior,

        All = Druid | Paladin | Priest | Shaman | Warrior | Rogue | Hunter | Mage | Warlock
    }
}
