﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable
{
    internal class CanEquipRangedWeaponTypeRule : SimpleRule
    {
        protected override string DisallowReason => "Class is unable to equip the item's type.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

        protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.Ranged;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            var equippableSpecs = item.Type switch
            {
                ItemType.Libram => Specializations.Paladin,
                ItemType.Idol => Specializations.Druid,
                ItemType.Totem => Specializations.Shaman,
                ItemType.Bow or ItemType.Crossbow or ItemType.Gun or ItemType.Thrown => Specializations.Rogue | Specializations.Warrior | Specializations.Hunter,
                ItemType.Wand => Specializations.Mage | Specializations.Warlock | Specializations.Priest,
                _ => throw new ArgumentException("Item is not a ranged item type.", nameof(item))
            };

            return (spec & equippableSpecs) == spec;
        }
    }
}