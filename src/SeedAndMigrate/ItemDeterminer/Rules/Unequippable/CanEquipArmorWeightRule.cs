// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable;

internal class CanEquipArmorWeightRule : SimpleRule
{
    protected override string DisallowReason => "Class is unable to equip the item's armor weight.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

    protected override bool AppliesTo(Item item)
    {
        return item.Type is ItemType.Cloth or ItemType.Leather or ItemType.Mail or ItemType.Plate;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        const Specializations mail = SpecializationGroups.Hunter | SpecializationGroups.Shaman;
        const Specializations leather = SpecializationGroups.Druid | SpecializationGroups.Rogue;
        const Specializations cloth = SpecializationGroups.Priest | SpecializationGroups.Mage | SpecializationGroups.Warlock;

        Specializations unequippable = item.Type switch
        {
            ItemType.Cloth => 0,
            ItemType.Leather => cloth,
            ItemType.Mail => cloth | leather,
            ItemType.Plate => cloth | leather | mail,
            _ => throw new ArgumentException("Rule does not apply to the given item.", nameof(item))
        };

        return (spec & unequippable) == 0;
    }
}
