// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class BindingLightRule : SimpleRule
{
    protected override string DisallowReason => "Binding Light has a healing effect.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item)
    {
        return item.Id is 47728u or 47947u;
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.All & ~SpecializationGroups.Healer;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        return false;
    }
}
