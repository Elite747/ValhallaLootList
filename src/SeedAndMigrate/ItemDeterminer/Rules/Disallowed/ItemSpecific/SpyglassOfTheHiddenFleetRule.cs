// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class SpyglassOfTheHiddenFleetRule : SimpleRule
    {
        protected override string DisallowReason => "Spyglass of the Hidden Fleet has a healing on-use effect, which is most appropriate for tanks.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 30620u;

        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.Tank;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
