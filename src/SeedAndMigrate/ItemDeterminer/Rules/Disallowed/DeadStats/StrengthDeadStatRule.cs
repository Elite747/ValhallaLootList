// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class StrengthDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.CasterDps | SpecializationGroups.Healer | SpecializationGroups.Hunter;
    }

    protected override int GetStat(Item item)
    {
        return item.Strength;
    }

    protected override string GetStatDisplayName()
    {
        return "Strength";
    }
}
