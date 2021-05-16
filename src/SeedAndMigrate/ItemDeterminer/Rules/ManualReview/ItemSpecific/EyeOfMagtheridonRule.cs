// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview.ItemSpecific
{
    internal class EyeOfMagtheridonRule : SimpleRule
    {
        protected override string DisallowReason => "Eye of Magtheridon has an effect that procs when spells are resisted, which is most useful to casters.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item) => item.Id == 28789u;

        protected override Specializations ApplicableSpecs() => Specializations.EnhanceShaman | Specializations.RetPaladin;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
