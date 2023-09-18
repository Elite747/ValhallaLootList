// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable;

internal class CanEquipOneHandWeaponRule : SimpleRule
{
    protected override string DisallowReason => "Class cannot equip the weapon's type.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

    protected override bool AppliesTo(Item item)
    {
        return item.Slot switch
        {
            InventorySlot.MainHand => true,
            InventorySlot.OneHand => true,
            InventorySlot.OffHand => item.Type is ItemType.Dagger or ItemType.Fist or ItemType.Axe or ItemType.Mace or ItemType.Sword,
            _ => false
        };
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        var equippableSpecs = item.Type switch
        {
            ItemType.Dagger => SpecializationGroups.All & ~(SpecializationGroups.Paladin | SpecializationGroups.DeathKnight),
            ItemType.Fist => SpecializationGroups.Druid | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Rogue | SpecializationGroups.Hunter,
            ItemType.Axe => SpecializationGroups.DeathKnight | SpecializationGroups.Paladin | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Hunter | SpecializationGroups.Rogue,
            ItemType.Mace => SpecializationGroups.DeathKnight | SpecializationGroups.Druid | SpecializationGroups.Paladin | SpecializationGroups.Priest | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Rogue,
            ItemType.Sword => SpecializationGroups.DeathKnight | SpecializationGroups.Paladin | SpecializationGroups.Warrior | SpecializationGroups.Rogue | SpecializationGroups.Hunter | SpecializationGroups.Mage | SpecializationGroups.Warlock,
            _ => throw new ArgumentException("Item is not a one-handed weapon.", nameof(item))
        };

        return (spec & equippableSpecs) == spec;
    }
}
