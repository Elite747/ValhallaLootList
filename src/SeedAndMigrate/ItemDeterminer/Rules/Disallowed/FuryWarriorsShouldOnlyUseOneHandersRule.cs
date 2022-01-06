// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class FuryWarriorsShouldOnlyUseOneHandersRule : SimpleRule
{
    protected override string DisallowReason => "Fury warriors should only use one-handed weapons.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Slot is InventorySlot.TwoHand;

    protected override Specializations ApplicableSpecs() => Specializations.FuryWarrior;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
