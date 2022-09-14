// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList;

public static class SpecializationsExtensions
{
    public static IEnumerable<Specializations> Split(this Specializations specs)
    {
        foreach (var spec in Enum.GetValues<Specializations>())
        {
            if (spec > 0 && (specs & spec) == spec)
            {
                yield return spec;
            }
        }
    }

    public static bool IsSingleSpecialization(this Specializations spec)
    {
        if ((long)spec == -2147483648)
        {
            return true; // Blood DK fix. WHY does this not parse correctly?
        }

        return spec > 0 && Enum.IsDefined(spec);
    }

    public static bool IsClass(this Specializations spec, Classes playerClass)
    {
        if (!IsSingleSpecialization(spec))
        {
            throw new ArgumentOutOfRangeException(nameof(spec), "Parameter must be a single defined specialization.");
        }

        return (spec & playerClass.ToSpecializations()) != 0;
    }

    public static string GetDisplayName(this Specializations spec, bool includeClassName = false, bool shorten = false)
    {
        string specName = spec switch
        {
            Specializations.BalanceDruid => "Balance",
            Specializations.BearDruid => "Guardian",
            Specializations.CatDruid => "Feral",
            Specializations.RestoDruid => "Restoration",
            Specializations.BeastMasterHunter => "Beast Mastery",
            Specializations.ArcaneMage => "Arcane",
            Specializations.HolyPaladin => "Holy",
            Specializations.ProtPaladin => "Protection",
            Specializations.RetPaladin => "Retribution",
            Specializations.DiscPriest => "Discipline",
            Specializations.ShadowPriest => "Shadow",
            Specializations.AssassinationRogue => "Assassination",
            Specializations.EleShaman => "Elemental",
            Specializations.EnhanceShaman => "Enhancement",
            Specializations.RestoShaman => "Restoration",
            Specializations.AfflictionWarlock => "Affliction",
            Specializations.ArmsWarrior => "Arms",
            Specializations.FuryWarrior => "Fury",
            Specializations.ProtWarrior => "Protection",
            Specializations.MarksmanHunter => "Marksmanship",
            Specializations.SurvivalHunter => "Survival",
            Specializations.FireMage => "Fire",
            Specializations.FrostMage => "Frost",
            Specializations.HolyPriest => "Holy",
            Specializations.CombatRogue => "Combat",
            Specializations.SubtletyRogue => "Subtlety",
            Specializations.DemoWarlock => "Demonology",
            Specializations.DestroWarlock => "Destruction",
            Specializations.BloodDeathKnight => shorten ? "Blood" : "Blood (DPS)",
            Specializations.BloodDeathKnightTank => shorten ? "Blood" : "Blood (Tank)",
            Specializations.FrostDeathKnight => shorten ? "Frost" : "Frost (DPS)",
            Specializations.FrostDeathKnightTank => shorten ? "Frost" : "Frost (Tank)",
            Specializations.UnholyDeathKnight => shorten ? "Unholy" : "Unholy (DPS)",
            Specializations.UnholyDeathKnightTank => shorten ? "Unholy" : "Unholy (Tank)",
            _ => throw new ArgumentOutOfRangeException(nameof(spec))
        };

        if (includeClassName)
        {
            return specName + " " + GetClassName(spec);
        }

        return specName;
    }

    public static Classes ToClass(this Specializations spec)
    {
        return spec switch
        {
            Specializations.BalanceDruid or Specializations.BearDruid or Specializations.CatDruid or Specializations.RestoDruid => Classes.Druid,
            Specializations.BeastMasterHunter or Specializations.MarksmanHunter or Specializations.SurvivalHunter => Classes.Hunter,
            Specializations.ArcaneMage or Specializations.FireMage or Specializations.FrostMage => Classes.Mage,
            Specializations.HolyPaladin or Specializations.ProtPaladin or Specializations.RetPaladin => Classes.Paladin,
            Specializations.DiscPriest or Specializations.ShadowPriest or Specializations.HolyPriest => Classes.Priest,
            Specializations.AssassinationRogue or Specializations.CombatRogue or Specializations.SubtletyRogue => Classes.Rogue,
            Specializations.EleShaman or Specializations.EnhanceShaman or Specializations.RestoShaman => Classes.Shaman,
            Specializations.AfflictionWarlock or Specializations.DemoWarlock or Specializations.DestroWarlock => Classes.Warlock,
            Specializations.ProtWarrior or Specializations.ArmsWarrior or Specializations.FuryWarrior => Classes.Warrior,
            Specializations.BloodDeathKnight or Specializations.BloodDeathKnightTank or
            Specializations.FrostDeathKnight or Specializations.FrostDeathKnightTank or
            Specializations.UnholyDeathKnight or Specializations.UnholyDeathKnightTank => Classes.DeathKnight,
            _ => throw new ArgumentOutOfRangeException(nameof(spec))
        };
    }

    public static string GetClassName(this Specializations spec)
    {
        return spec switch
        {
            Specializations.BalanceDruid or Specializations.BearDruid or Specializations.CatDruid or Specializations.RestoDruid => "Druid",
            Specializations.BeastMasterHunter or Specializations.MarksmanHunter or Specializations.SurvivalHunter => "Hunter",
            Specializations.ArcaneMage or Specializations.FireMage or Specializations.FrostMage => "Mage",
            Specializations.HolyPaladin or Specializations.ProtPaladin or Specializations.RetPaladin => "Paladin",
            Specializations.DiscPriest or Specializations.ShadowPriest or Specializations.HolyPriest => "Priest",
            Specializations.AssassinationRogue or Specializations.CombatRogue or Specializations.SubtletyRogue => "Rogue",
            Specializations.EleShaman or Specializations.EnhanceShaman or Specializations.RestoShaman => "Shaman",
            Specializations.AfflictionWarlock or Specializations.DemoWarlock or Specializations.DestroWarlock => "Warlock",
            Specializations.ProtWarrior or Specializations.ArmsWarrior or Specializations.FuryWarrior => "Warrior",
            Specializations.BloodDeathKnight or Specializations.BloodDeathKnightTank or
            Specializations.FrostDeathKnight or Specializations.FrostDeathKnightTank or
            Specializations.UnholyDeathKnight or Specializations.UnholyDeathKnightTank => "Death Knight",
            _ => throw new ArgumentOutOfRangeException(nameof(spec))
        };
    }

    public static bool IsTank(this Specializations spec)
    {
        return (spec & SpecializationGroups.Tank) != 0;
    }

    public static bool IsHealer(this Specializations spec)
    {
        return (spec & SpecializationGroups.Healer) != 0;
    }

    public static bool IsDps(this Specializations spec)
    {
        return (spec & SpecializationGroups.Dps) != 0;
    }

    public static RaidRole GetRole(this Specializations spec)
    {
        if (IsTank(spec))
        {
            return RaidRole.Tank;
        }
        if (IsHealer(spec))
        {
            return RaidRole.Healer;
        }
        if (IsDps(spec))
        {
            return RaidRole.Dps;
        }
        return RaidRole.Unknown;
    }

    public static int GetSortingIndex(this Specializations spec)
    {
        return spec switch
        {
            // tanks
            Specializations.BloodDeathKnightTank => 0,
            Specializations.FrostDeathKnightTank => 0,
            Specializations.UnholyDeathKnightTank => 0,
            Specializations.BearDruid => 1,
            Specializations.ProtPaladin => 2,
            Specializations.ProtWarrior => 3,

            // healers
            Specializations.RestoDruid => 4,
            Specializations.DiscPriest => 5,
            Specializations.HolyPriest => 5,
            Specializations.HolyPaladin => 6,
            Specializations.RestoShaman => 7,

            // dps
            Specializations.BloodDeathKnight => 8,
            Specializations.FrostDeathKnight => 8,
            Specializations.UnholyDeathKnight => 8,

            Specializations.BalanceDruid => 9,
            Specializations.CatDruid => 9,

            Specializations.BeastMasterHunter => 10,
            Specializations.MarksmanHunter => 10,
            Specializations.SurvivalHunter => 10,

            Specializations.ArcaneMage => 13,
            Specializations.FireMage => 13,
            Specializations.FrostMage => 13,

            Specializations.RetPaladin => 16,

            Specializations.ShadowPriest => 17,

            Specializations.AssassinationRogue => 18,
            Specializations.CombatRogue => 18,
            Specializations.SubtletyRogue => 18,

            Specializations.EleShaman => 21,
            Specializations.EnhanceShaman => 21,

            Specializations.AfflictionWarlock => 23,
            Specializations.DemoWarlock => 23,
            Specializations.DestroWarlock => 23,

            Specializations.ArmsWarrior => 26,
            Specializations.FuryWarrior => 26,

            _ => int.MaxValue
        };
    }
}
