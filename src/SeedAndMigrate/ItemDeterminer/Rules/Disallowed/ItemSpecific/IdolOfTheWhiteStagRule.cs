// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class IdolOfTheWhiteStagRule : SimpleRule
{
    protected override string DisallowReason => "Idol of the Crescent Goddess is only appropriate for the Feral and Guardian specs.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Id == 32257u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.Druid;

    protected override bool IsAllowed(Item item, Specializations spec) => spec is Specializations.BearDruid or Specializations.CatDruid;
}
