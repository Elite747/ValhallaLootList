// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview;

internal class TankGearShouldHaveDefensiveStatsRule : SimpleRule
{
    protected override string DisallowReason => "Tank gear should have defensive stats.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

    protected override bool AppliesTo(Item item)
    {
        return item.Slot switch
        {
            InventorySlot.Head => true,
            InventorySlot.Neck => true,
            InventorySlot.Shoulder => true,
            InventorySlot.Shirt => true,
            InventorySlot.Chest => true,
            InventorySlot.Waist => true,
            InventorySlot.Legs => true,
            InventorySlot.Feet => true,
            InventorySlot.Wrist => true,
            InventorySlot.Hands => true,
            InventorySlot.Finger => true,
            InventorySlot.Back => true,
            _ => false
        };
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Tank;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        if (item.BlockRating > 0 ||
            item.BlockValue > 0 ||
            item.Defense > 0 ||
            item.Dodge > 0 ||
            item.Parry > 0)
        {
            return true;
        }

        if (spec == Specializations.BearDruid && item.Type == ItemType.Leather)
        {
            // Bear druids may use leather that has no pure-defensive stats.
            return item.Stamina > 0;
        }

        return false;
    }
}
