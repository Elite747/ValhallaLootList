// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.ItemSpecific
{
    internal class PrismOfInnerCalmRule : SimpleRule
    {
        protected override string DisallowReason => "Prism of Inner Calm reduces threat on critical strikes, which is only useful to dps.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 30621u;

        protected override Specializations ApplicableSpecs() => Specializations.Tank | Specializations.Healer;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
