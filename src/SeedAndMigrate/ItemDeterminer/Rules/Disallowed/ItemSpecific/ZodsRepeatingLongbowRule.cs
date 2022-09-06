// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class ZodsRepeatingLongbowRule : SimpleRule
{
    protected override string DisallowReason => "Zod's Repeating Longbow has a ranged attack proc effect.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Id is 50034u or 50638u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.Hunter;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
