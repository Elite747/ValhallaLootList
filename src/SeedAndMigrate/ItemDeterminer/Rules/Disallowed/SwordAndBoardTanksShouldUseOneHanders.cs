﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class SwordAndBoardTanksShouldUseOneHanders : SimpleRule
{
    protected override string DisallowReason => "Protection warriors and paladins should use one-handed weapons.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item)
    {
        return item.Slot is InventorySlot.TwoHand;
    }

    protected override Specializations ApplicableSpecs()
    {
        return Specializations.ProtPaladin | Specializations.ProtWarrior;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        return false;
    }
}
