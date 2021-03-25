// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class BlackBowOfTheBetrayerRule : SimpleRule
    {
        protected override string DisallowReason => "Black Bow of the Betrayer has a mana drain effect, which is only useful to hunters.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 32336u;

        protected override Specializations ApplicableSpecs() => Specializations.All & ~Specializations.Hunter;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
