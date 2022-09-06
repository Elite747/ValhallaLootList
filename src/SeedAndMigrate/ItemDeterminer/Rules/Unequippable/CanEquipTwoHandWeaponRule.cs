// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable;

internal class CanEquipTwoHandWeaponRule : SimpleRule
{
    protected override string DisallowReason => "Class cannot equip the weapon's type.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

    protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.TwoHand;

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        var equippableSpecs = item.Type switch
        {
            ItemType.Axe => SpecializationGroups.DeathKnight | SpecializationGroups.Paladin | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Hunter,
            ItemType.Mace => SpecializationGroups.DeathKnight | SpecializationGroups.Druid | SpecializationGroups.Paladin | SpecializationGroups.Shaman | SpecializationGroups.Warrior,
            ItemType.Sword => SpecializationGroups.DeathKnight | SpecializationGroups.Paladin | SpecializationGroups.Warrior | SpecializationGroups.Hunter,
            ItemType.Polearm => SpecializationGroups.DeathKnight | SpecializationGroups.Druid | SpecializationGroups.Paladin | SpecializationGroups.Warrior | SpecializationGroups.Hunter,
            ItemType.Stave => SpecializationGroups.All & ~(SpecializationGroups.Paladin | SpecializationGroups.Rogue | SpecializationGroups.DeathKnight),
            _ => throw new ArgumentException("Item is not a two-handed weapon.", nameof(item))
        };

        return (spec & equippableSpecs) == spec;
    }
}
