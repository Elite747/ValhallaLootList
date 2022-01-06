// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class FirePowerDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~(SpecializationGroups.Mage | Specializations.EleShaman | SpecializationGroups.Warlock);

    protected override int GetStat(Item item)
    {
        return item.Id switch
        {
            30020u => 60, // Fire-Cord of the Magus
            32589u => 51, // Hellfire-Encased Pendant
            _ => 0
        };
    }

    protected override string GetStatDisplayName() => "Fire Power";
}
