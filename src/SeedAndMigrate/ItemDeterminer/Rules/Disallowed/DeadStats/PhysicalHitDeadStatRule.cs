﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats
{
    internal class PhysicalHitDeadStatRule : DeadStatRule
    {
        protected override Specializations ApplicableSpecs() => SpecializationGroups.CasterDps | SpecializationGroups.Healer;

        protected override int GetStat(Item item) => item.PhysicalHit;

        protected override string GetStatDisplayName() => "Hit";
    }
}
