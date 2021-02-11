// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.FactualLimitations
{
    internal class CanDualWieldRule : SimpleRule
    {
        protected override string DisallowReason => "Class is unable to dual wield weapons.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

        protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.OffHand && item.Type != ItemType.Other && item.Type != ItemType.Shield;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            return (spec & (Specializations.Warrior | Specializations.EnhanceShaman | Specializations.Rogue | Specializations.Hunter)) == spec;
        }
    }
}
