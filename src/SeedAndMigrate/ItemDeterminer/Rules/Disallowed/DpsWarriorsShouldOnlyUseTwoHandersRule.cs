// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class DpsWarriorsShouldOnlyUseTwoHandersRule : SimpleRule
{
    protected override string DisallowReason => "Dps warriors should only use two-handed weapons.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item)
    {
        return item.Slot is InventorySlot.OneHand or InventorySlot.MainHand or InventorySlot.OffHand;
    }

    protected override Specializations ApplicableSpecs() => Specializations.ArmsWarrior | Specializations.FuryWarrior;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
