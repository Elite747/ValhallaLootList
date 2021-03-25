// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview.ItemSpecific
{
    internal class FathomBroochOfTheTidewalkerRule : SimpleRule
    {
        protected override string DisallowReason => "Fathom-Brooch of the Tidewalker is more valuable to healers than dps.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 30663u;

        protected override Specializations ApplicableSpecs() => Specializations.EleShaman | Specializations.EnhanceShaman;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
