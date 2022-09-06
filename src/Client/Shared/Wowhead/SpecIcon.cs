// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared;

public class SpecIcon : WowIcon
{
    [Parameter] public Specializations Spec { get; set; }

    protected override string GetIconId()
    {
        return Spec switch
        {
            Specializations.BalanceDruid => "spell_nature_starfall",
            Specializations.BearDruid => "ability_racial_bearform",
            Specializations.CatDruid => "ability_druid_catform",
            Specializations.RestoDruid => "spell_nature_healingtouch",
            Specializations.BeastMasterHunter => "ability_hunter_bestialdiscipline",
            Specializations.ArcaneMage => "spell_holy_magicalsentry",
            Specializations.HolyPaladin => "spell_holy_holybolt",
            Specializations.ProtPaladin => "ability_paladin_shieldofthetemplar",
            Specializations.RetPaladin => "spell_holy_auraoflight",
            Specializations.DiscPriest => "spell_holy_powerwordshield",
            Specializations.ShadowPriest => "spell_shadow_shadowwordpain",
            Specializations.AssassinationRogue => "ability_rogue_deadlybrew",
            Specializations.EleShaman => "spell_nature_lightning",
            Specializations.EnhanceShaman => "spell_shaman_improvedstormstrike",
            Specializations.RestoShaman => "spell_nature_magicimmunity",
            Specializations.AfflictionWarlock => "spell_shadow_deathcoil",
            Specializations.ProtWarrior => "ability_warrior_defensivestance",
            Specializations.ArmsWarrior => "ability_warrior_savageblow",
            Specializations.FuryWarrior => "ability_warrior_innerrage",
            Specializations.MarksmanHunter => "ability_hunter_focusedaim",
            Specializations.SurvivalHunter => "ability_hunter_camouflage",
            Specializations.FireMage => "spell_fire_firebolt02",
            Specializations.FrostMage => "spell_frost_frostbolt02",
            Specializations.HolyPriest => "spell_holy_guardianspirit",
            Specializations.CombatRogue => "inv_sword_30",
            Specializations.SubtletyRogue => "ability_stealth",
            Specializations.DemoWarlock => "spell_shadow_metamorphosis",
            Specializations.DestroWarlock => "spell_shadow_rainoffire",
            Specializations.BloodDeathKnight or Specializations.BloodDeathKnightTank => "spell_deathknight_bloodpresence",
            Specializations.FrostDeathKnight or Specializations.FrostDeathKnightTank => "spell_deathknight_frostpresence",
            Specializations.UnholyDeathKnight or Specializations.UnholyDeathKnightTank => "spell_deathknight_unholypresence",
            _ => throw new ArgumentOutOfRangeException(nameof(Spec))
        };
    }

    protected override string GetAltText()
    {
        return Spec.GetDisplayName(includeClassName: true);
    }
}
