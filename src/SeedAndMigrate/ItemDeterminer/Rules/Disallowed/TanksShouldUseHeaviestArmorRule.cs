// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed
{
    internal class TanksShouldUseHeaviestArmorRule : SimpleRule
    {
        protected override string DisallowReason => "Tanks should not equip lesser armor types.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item)
        {
            return item.Type is ItemType.Cloth or ItemType.Leather or ItemType.Mail or ItemType.Plate;
        }

        protected override Specializations ApplicableSpecs() => SpecializationGroups.Tank;

        protected override bool IsAllowed(Item item, Specializations spec)
        {
            if (spec == Specializations.BearDruid)
            {
                return item.Type == ItemType.Leather;
            }
            else
            {
                return item.Type == ItemType.Plate;
            }
        }
    }
}
