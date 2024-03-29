﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class Mp5DeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs()
    {
        return Specializations.BearDruid | Specializations.CatDruid | SpecializationGroups.Rogue | SpecializationGroups.Warrior | SpecializationGroups.DeathKnight;
    }

    protected override int GetStat(Item item)
    {
        return item.ManaPer5;
    }

    protected override string GetStatDisplayName()
    {
        return "MP5";
    }
}
