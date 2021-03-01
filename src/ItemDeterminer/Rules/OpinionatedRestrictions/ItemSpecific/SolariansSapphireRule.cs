// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.OpinionatedRestrictions.ItemSpecific
{
    internal class SolariansSapphireRule : SimpleRule
    {
        protected override string DisallowReason => "Solarian's Sapphire is more valuable to tanks than dps.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 30446u;

        protected override Specializations ApplicableSpecs() => Specializations.ArmsWarrior | Specializations.FuryWarrior;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
