// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed;

internal class HuntersShouldNotUseThrownRule : SimpleRule
{
    protected override string DisallowReason => "Hunter ranged abilities do not work with thrown weapons.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

    protected override bool AppliesTo(Item item) => item.Type == ItemType.Thrown;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.Hunter;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
