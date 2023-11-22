// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Unequippable;

internal class CanEquipShieldRule : SimpleRule
{
    protected override string DisallowReason => "Class cannot equip shields.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Unequippable;

    protected override bool AppliesTo(Item item)
    {
        return item.Type == ItemType.Shield;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        return (spec & (SpecializationGroups.Warrior | SpecializationGroups.Paladin | SpecializationGroups.Shaman)) == spec;
    }
}
