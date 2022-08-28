// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

[Flags]
public enum Specializations : long
{
    None = 0,
    BalanceDruid = 1 << 0,
    BearDruid = 1 << 1,
    CatDruid = 1 << 2,
    RestoDruid = 1 << 3,
    BeastMasterHunter = 1 << 4,
    ArcaneMage = 1 << 5,
    HolyPaladin = 1 << 6,
    ProtPaladin = 1 << 7,
    RetPaladin = 1 << 8,
    DiscPriest = 1 << 9,
    ShadowPriest = 1 << 10,
    AssassinationRogue = 1 << 11,
    EleShaman = 1 << 12,
    EnhanceShaman = 1 << 13,
    RestoShaman = 1 << 14,
    AfflictionWarlock = 1 << 15,
    ProtWarrior = 1 << 16,
    ArmsWarrior = 1 << 17,
    FuryWarrior = 1 << 18,
    MarksmanHunter = 1 << 19,
    SurvivalHunter = 1 << 20,
    FireMage = 1 << 21,
    FrostMage = 1 << 22,
    HolyPriest = 1 << 23,
    CombatRogue = 1 << 24,
    SubtletyRogue = 1 << 25,
    DemoWarlock = 1 << 26,
    DestroWarlock = 1 << 27,
    BloodDeathKnight = 1 << 28,
    FrostDeathKnight = 1 << 29,
    UnholyDeathKnight = 1 << 30,
    BloodDeathKnightTank = 1 << 31,
    FrostDeathKnightTank = 1L << 32,
    UnholyDeathKnightTank = 1L << 33
}
