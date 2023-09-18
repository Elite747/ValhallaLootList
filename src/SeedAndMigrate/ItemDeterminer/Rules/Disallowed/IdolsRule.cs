// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class IdolsRule : Rule
{
    protected override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        return item.Id switch
        {
            40342u when spec != Specializations.RestoDruid => new(spec, DeterminationLevel.Disallowed, "Idol of Awakening is meant for restoration druids."),
            45509u when spec is not Specializations.BearDruid and not Specializations.CatDruid => new(spec, DeterminationLevel.Disallowed, "Idol of the Corruptor is meant for feral druids."),
            45270u when spec != Specializations.BalanceDruid => new(spec, DeterminationLevel.Disallowed, "Idol of the Crying Wind is meant for balance druids."),
            46138u when spec != Specializations.RestoDruid => new(spec, DeterminationLevel.Disallowed, "Idol of the Flourishing Life is meant for restoration druids."),
            40321u when spec != Specializations.BalanceDruid => new(spec, DeterminationLevel.Disallowed, "Idol of the Shooting Star is meant for balance druids."),
            39757u when spec is not Specializations.BearDruid and not Specializations.CatDruid => new(spec, DeterminationLevel.Disallowed, "Idol of Worship is meant for feral druids."),
            _ => new(spec, DeterminationLevel.Allowed, string.Empty)
        };
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.Druid;
    }

    protected override bool AppliesTo(Item item)
    {
        return item.Type == ItemType.Idol;
    }
}
