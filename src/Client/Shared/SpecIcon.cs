// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared
{
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
                Specializations.Hunter => "class_hunter",
                Specializations.Mage => "class_mage",
                Specializations.HolyPaladin => "spell_holy_holybolt",
                Specializations.ProtPaladin => "ability_paladin_shieldofthetemplar",
                Specializations.RetPaladin => "spell_holy_auraoflight",
                Specializations.HealerPriest => "spell_holy_guardianspirit",
                Specializations.ShadowPriest => "spell_shadow_shadowwordpain",
                Specializations.Rogue => "class_rogue",
                Specializations.EleShaman => "spell_nature_lightning",
                Specializations.EnhanceShaman => "spell_nature_lightningshield",
                Specializations.RestoShaman => "spell_nature_magicimmunity",
                Specializations.Warlock => "class_warlock",
                Specializations.ProtWarrior => "ability_warrior_defensivestance",
                Specializations.ArmsWarrior => "ability_warrior_savageblow",
                Specializations.FuryWarrior => "ability_warrior_innerrage",
                _ => throw new ArgumentOutOfRangeException(nameof(Spec))
            };
        }

        protected override string GetAltText()
        {
            return Spec switch
            {
                Specializations.BalanceDruid => "Balance Druid",
                Specializations.BearDruid => "Feral Druid (Bear)",
                Specializations.CatDruid => "Feral Druid (Cat)",
                Specializations.RestoDruid => "Restoration Druid",
                Specializations.Hunter => "Hunter",
                Specializations.Mage => "Mage",
                Specializations.HolyPaladin => "Holy Paladin",
                Specializations.ProtPaladin => "Protection Paladin",
                Specializations.RetPaladin => "Retribution Paladin",
                Specializations.HealerPriest => "Healer Priest",
                Specializations.ShadowPriest => "Shadow Priest",
                Specializations.Rogue => "Rogue",
                Specializations.EleShaman => "Elemental Shaman",
                Specializations.EnhanceShaman => "Enhancement Shaman",
                Specializations.RestoShaman => "Restoration Shaman",
                Specializations.Warlock => "Warlock",
                Specializations.ProtWarrior => "Protection Warrior",
                Specializations.ArmsWarrior => "Arms Warrior",
                Specializations.FuryWarrior => "Fury Warrior",
                _ => throw new ArgumentOutOfRangeException(nameof(Spec))
            };
        }
    }
}