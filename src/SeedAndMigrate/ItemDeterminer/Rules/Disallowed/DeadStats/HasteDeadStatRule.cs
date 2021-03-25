﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats
{
    internal class HasteDeadStatRule : DeadStatRule
    {
        // TODO: Is unspecified haste physical haste, melee haste, or all haste?
        protected override Specializations ApplicableSpecs() => Specializations.Healer | Specializations.CasterDps;

        protected override int GetStat(Item item) => item.Haste;

        protected override string GetStatDisplayName() => "Haste";
    }
}