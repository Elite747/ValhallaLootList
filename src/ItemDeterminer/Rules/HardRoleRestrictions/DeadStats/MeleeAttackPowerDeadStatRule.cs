﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats
{
    internal class MeleeAttackPowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.Healer | Specializations.CasterDps | Specializations.Hunter;

        protected override int GetStat(Item item) => item.MeleeAttackPower;

        protected override string GetStatDisplayName() => "Melee Attack Power";

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            return spec == Specializations.Hunter && item.RangedAttackPower > 0;
        }
    }
}
