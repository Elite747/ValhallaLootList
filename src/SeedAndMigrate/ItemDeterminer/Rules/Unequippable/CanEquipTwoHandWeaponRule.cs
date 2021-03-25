// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable
{
    internal class CanEquipTwoHandWeaponRule : SimpleRule
    {
        protected override string DisallowReason => "Class cannot equip the weapon's type.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

        protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.TwoHand;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            var equippableSpecs = item.Type switch
            {
                ItemType.Axe => Specializations.Paladin | Specializations.Shaman | Specializations.Warrior | Specializations.Hunter,
                ItemType.Mace => Specializations.Druid | Specializations.Paladin | Specializations.Shaman | Specializations.Warrior,
                ItemType.Sword => Specializations.Paladin | Specializations.Warrior | Specializations.Hunter,
                ItemType.Polearm => Specializations.Paladin | Specializations.Warrior | Specializations.Hunter,
                ItemType.Stave => Specializations.All & ~(Specializations.Paladin | Specializations.Rogue),
                _ => throw new ArgumentException("Item is not a two-handed weapon.", nameof(item))
            };

            return (spec & equippableSpecs) == spec;
        }
    }
}
