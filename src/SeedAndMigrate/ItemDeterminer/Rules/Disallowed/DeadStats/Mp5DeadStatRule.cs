﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats
{
    internal class Mp5DeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => Specializations.BearDruid | Specializations.CatDruid | SpecializationGroups.Rogue | SpecializationGroups.Warrior;

        protected override int GetStat(Item item) => item.ManaPer5;

        protected override string GetStatDisplayName() => "MP5";
    }
}
