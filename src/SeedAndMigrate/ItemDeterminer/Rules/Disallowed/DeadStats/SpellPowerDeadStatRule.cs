﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats;

internal class SpellPowerDeadStatRule : DeadStatRule
{
    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Tank | SpecializationGroups.PhysicalDps;
    }

    protected override int GetStat(Item item)
    {
        return item.SpellPower;
    }

    protected override string GetStatDisplayName()
    {
        return "Spell Power";
    }
}
