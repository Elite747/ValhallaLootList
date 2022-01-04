// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class TotemOfTheMaelstromRule : SimpleRule
{
    protected override string DisallowReason => "Totem of the Maelstrom is only appropriate for the Restoration spec.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Id == 30023u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.Shaman;

    protected override bool IsAllowed(Item item, Specializations spec) => spec == Specializations.RestoShaman;
}
