// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.FactualLimitations
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
                ItemType.Dagger => (Specializations.All & ~Specializations.Paladin),
                ItemType.Fist => Specializations.Druid | Specializations.Shaman | Specializations.Warrior | Specializations.Rogue | Specializations.Hunter,
                ItemType.Axe => Specializations.Paladin | Specializations.Shaman | Specializations.Warrior | Specializations.Hunter,
                ItemType.Mace => Specializations.Druid | Specializations.Paladin | Specializations.Priest | Specializations.Shaman | Specializations.Warrior | Specializations.Rogue,
                ItemType.Sword => Specializations.Paladin | Specializations.Warrior | Specializations.Rogue | Specializations.Hunter | Specializations.Mage | Specializations.Warlock,
                _ => throw new ArgumentException("Item is not a one-handed weapon.", nameof(item))
            };

            return (spec & equippableSpecs) == spec;
        }
    }
}
