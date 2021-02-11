// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.DeadStats
{
    internal class RangedAttackPowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.Hunter;

        protected override int GetStat(Item item) => item.RangedAttackPower;

        protected override string GetStatDisplayName() => "Ranged Attack Power";

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            return (spec & (Specializations.MeleeDps | Specializations.Tank)) != 0 && item.MeleeAttackPower > 0;
        }
    }
}
