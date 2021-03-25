// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed
{
    internal class DualWieldersShouldNotUseOffHandsRule : SimpleRule
    {
        protected override string DisallowReason => "Dual-wielding specializations should not use non-weapon offhands.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.OffHand && (item.Type == ItemType.Other || item.Type == ItemType.Shield);

        protected override Specializations ApplicableSpecs() => Specializations.FuryWarrior | Specializations.EnhanceShaman | Specializations.Rogue;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
