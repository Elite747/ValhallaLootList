// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific;

internal class QuelSerrarRule : SimpleRule
{
    protected override string DisallowReason => "Quel'Serrar has a tanking proc effect.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item)
    {
        return item.Id is 49495u or 49303u;
    }

    protected override Specializations ApplicableSpecs()
    {
        return SpecializationGroups.All & ~SpecializationGroups.Tank;
    }

    protected override bool IsAllowed(Item item, Specializations spec)
    {
        return false;
    }
}
