// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview.ItemSpecific
{
    internal class ShiftingNaaruSliverRule : SimpleRule
    {
        protected override string DisallowReason => "Shifting Naaru Sliver is more valuable to caster dps than healers.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 34429u;

        protected override Specializations ApplicableSpecs() => Specializations.Healer;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
