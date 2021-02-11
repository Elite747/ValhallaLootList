﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats
{
    internal class HealingPowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.Healer;

        protected override int GetStat(Item item) => item.HealingPower;

        protected override string GetStatDisplayName() => "Healing Power";

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            return item.SpellPower == item.HealingPower && (spec & (Specializations.CasterDps | Specializations.PhysicalCaster)) != 0;
        }
    }
}
