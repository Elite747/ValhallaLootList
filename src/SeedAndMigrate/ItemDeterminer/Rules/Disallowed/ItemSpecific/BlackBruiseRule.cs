// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class BlackBruiseRule : SimpleRule
{
    protected override string DisallowReason => "Black Bruise has a melee attack proc effect.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Id is 50035u or 50692u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.MeleeDps;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
