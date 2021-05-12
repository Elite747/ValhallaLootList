// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats
{
    internal class RangedAttackPowerDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.Hunter;

        protected override int GetStat(Item item) => item.RangedAttackPower;

        protected override string GetStatDisplayName() => "Ranged Attack Power";

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            return (spec & (SpecializationGroups.MeleeDps | SpecializationGroups.Tank)) != 0 && item.MeleeAttackPower > 0;
        }
    }
}
