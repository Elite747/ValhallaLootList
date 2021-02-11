// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions
{
    internal class ArmsWarriorsShouldOnlyUseTwoHandersRule : SimpleRule
    {
        protected override string DisallowReason => "Arms warriors should only use two-handed weapons.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item)
        {
            return item.Slot is InventorySlot.OneHand or InventorySlot.MainHand or InventorySlot.OffHand;
        }

        protected override Specializations ApplicableSpecs() => Specializations.ArmsWarrior;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
