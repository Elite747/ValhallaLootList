// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class TomeOfTheLightbringerRule : SimpleRule
    {
        protected override string DisallowReason => "Tome of the Lightbringer has a proc that gives Shield Block Value, which is only useful for the Protection spec.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 32368u;

        protected override Specializations ApplicableSpecs() => Specializations.HolyPaladin | Specializations.RetPaladin;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
