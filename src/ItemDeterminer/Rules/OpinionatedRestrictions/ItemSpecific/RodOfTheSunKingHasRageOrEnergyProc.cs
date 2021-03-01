// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.OpinionatedRestrictions.ItemSpecific
{
    internal class RodOfTheSunKingHasRageOrEnergyProcRule : SimpleRule
    {
        protected override string DisallowReason => "Rod of the Sun King gives rage or energy on hit, which only benefits Rogues and Warriors.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 29996u;

        protected override Specializations ApplicableSpecs() => Specializations.All & ~(Specializations.Warrior | Specializations.Rogue);

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
