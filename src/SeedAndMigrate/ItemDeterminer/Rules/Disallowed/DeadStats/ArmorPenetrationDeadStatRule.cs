﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class ArmorPenetrationDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Healer | SpecializationGroups.CasterDps;
    }

    protected override int GetStat(Item item)
    {
        return item.ArmorPenetration;
    }

    protected override string GetStatDisplayName()
    {
        return "Armor Penetration";
    }
}
