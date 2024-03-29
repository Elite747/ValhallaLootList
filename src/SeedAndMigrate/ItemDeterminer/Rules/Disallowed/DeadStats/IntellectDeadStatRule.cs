﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class IntellectDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Warrior | SpecializationGroups.Rogue | SpecializationGroups.DeathKnight;
    }

    protected override int GetStat(Item item)
    {
        return item.Intellect;
    }

    protected override string GetStatDisplayName()
    {
        return "Intellect";
    }
}
