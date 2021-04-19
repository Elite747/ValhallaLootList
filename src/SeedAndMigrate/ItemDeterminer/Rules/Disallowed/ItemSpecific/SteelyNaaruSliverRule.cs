// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class SteelyNaaruSliverRule : SimpleRule
    {
        protected override string DisallowReason => "Steely Naaru Sliver has an on-use effect that raises maximum HP, which is most appropriate for tanks.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 34428u;

        protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.Tank;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
