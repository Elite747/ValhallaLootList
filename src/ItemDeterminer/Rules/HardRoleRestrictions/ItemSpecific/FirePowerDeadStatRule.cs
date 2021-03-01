﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.ItemSpecific
{
    internal class FirePowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~(Specializations.Mage | Specializations.EleShaman | Specializations.Warlock);

        protected override int GetStat(Item item)
        {
            return item.Id switch
            {
                30020u => 60, // Fire-Cord of the Magus
                32589u => 51, // Hellfire-Encased Pendant
                _ => 0
            };
        }

        protected override string GetStatDisplayName() => "Fire Power";
    }
}
