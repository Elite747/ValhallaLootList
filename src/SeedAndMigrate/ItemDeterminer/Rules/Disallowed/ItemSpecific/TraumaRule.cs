// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class TraumaRule : SimpleRule
{
    protected override string DisallowReason => "Trauma has a healing proc effect.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Id is 50028u or 50685u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.Healer;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
