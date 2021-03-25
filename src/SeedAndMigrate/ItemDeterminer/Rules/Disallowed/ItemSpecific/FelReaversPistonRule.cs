// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class FelReaversPistonRule : SimpleRule
    {
        protected override string DisallowReason => "Fel Reaver's Piston has an on-heal proc, which is only appropriate for healer specs.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 30619u;

        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.Healer;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
