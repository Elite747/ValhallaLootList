﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable
{
    internal class CanEquipOneHandWeaponRule : SimpleRule
    {
        protected override string DisallowReason => "Class cannot equip the weapon's type.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

        protected override bool AppliesTo(Item item) => item.Slot is InventorySlot.MainHand or InventorySlot.OneHand;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            var equippableSpecs = item.Type switch
            {
                ItemType.Dagger => (SpecializationGroups.All & ~SpecializationGroups.Paladin),
                ItemType.Fist => SpecializationGroups.Druid | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Rogue | SpecializationGroups.Hunter,
                ItemType.Axe => SpecializationGroups.Paladin | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Hunter,
                ItemType.Mace => SpecializationGroups.Druid | SpecializationGroups.Paladin | SpecializationGroups.Priest | SpecializationGroups.Shaman | SpecializationGroups.Warrior | SpecializationGroups.Rogue,
                ItemType.Sword => SpecializationGroups.Paladin | SpecializationGroups.Warrior | SpecializationGroups.Rogue | SpecializationGroups.Hunter | SpecializationGroups.Mage | SpecializationGroups.Warlock,
                _ => throw new ArgumentException("Item is not a one-handed weapon.", nameof(item))
            };

            return (spec & equippableSpecs) == spec;
        }
    }
}
