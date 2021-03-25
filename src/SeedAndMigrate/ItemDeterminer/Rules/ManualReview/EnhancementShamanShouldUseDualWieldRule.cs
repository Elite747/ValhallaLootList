// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview
{
    internal class EnhancementShamanShouldUseDualWieldRule : SimpleRule
    {
        protected override string DisallowReason => "Enhancement shamans should utilize dual-wield specializations instead of two-handed.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Slot == InventorySlot.TwoHand;

        protected override Specializations ApplicableSpecs() => Specializations.EnhanceShaman;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
