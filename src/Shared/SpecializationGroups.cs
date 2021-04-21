// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using static ValhallaLootList.Specializations;

namespace ValhallaLootList
{
    public static class SpecializationGroups
    {
        public const Specializations
            Tank = BearDruid | ProtPaladin | ProtWarrior,
            Healer = RestoDruid | RestoShaman | HolyPaladin | DiscPriest | HolyPriest,
            MeleeDps = CatDruid | Rogue | EnhanceShaman | RetPaladin | ArmsWarrior | FuryWarrior,
            CasterDps = BalanceDruid | Mage | ShadowPriest | EleShaman | Warlock,
            PhysicalDps = MeleeDps | Hunter,
            PhysicalCaster = ProtPaladin | EnhanceShaman | RetPaladin,
            Dps = CasterDps | PhysicalDps,

            HealerPriest = DiscPriest | HolyPriest,

            Druid = BalanceDruid | BearDruid | CatDruid | RestoDruid,
            Hunter = BeastMasterHunter | MarksmanHunter | SurvivalHunter,
            Mage = ArcaneMage | FireMage | FrostMage,
            Paladin = HolyPaladin | ProtPaladin | RetPaladin,
            Priest = DiscPriest | HolyPriest | ShadowPriest,
            Rogue = AssassinationRogue | CombatRogue | SubtletyRogue,
            Shaman = EleShaman | EnhanceShaman | RestoShaman,
            Warlock = AfflictionWarlock | DemoWarlock | DestroWarlock,
            Warrior = ProtWarrior | ArmsWarrior | FuryWarrior,

            All = Druid | Paladin | Priest | Shaman | Warrior | Rogue | Hunter | Mage | Warlock;
    }
}
