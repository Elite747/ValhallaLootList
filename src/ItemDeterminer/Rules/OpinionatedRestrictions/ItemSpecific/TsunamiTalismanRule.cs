// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.OpinionatedRestrictions.ItemSpecific
{
    internal class TsunamiTalismanRule : SimpleRule
    {
        protected override string DisallowReason => "Tsunami Talisman is more valuable to physical dps than tanks.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 30627u;

        protected override Specializations ApplicableSpecs() => Specializations.Tank;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
