// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable;

internal class CanDualWieldRule : SimpleRule
{
    protected override string DisallowReason => "Class is unable to dual wield weapons.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

    protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.OffHand && item.Type != ItemType.Other && item.Type != ItemType.Shield;

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        // While only enhancement shaman are able to equip offhand weapons, don't report items as unequippable for non-enhancement shaman
        // as it will cause those items to not be selectable in loot list creation.
        return (spec & (SpecializationGroups.Warrior | SpecializationGroups.Shaman | SpecializationGroups.Rogue | SpecializationGroups.Hunter | SpecializationGroups.DeathKnight)) == spec;
    }
}
