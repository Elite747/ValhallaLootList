// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.ItemSpecific
{
    internal class IdolOfTheCrescentGoddessRule : SimpleRule
    {
        protected override string DisallowReason => "Idol of the Crescent Goddess is only appropriate for the Restoration spec.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 30051u;

        protected override Specializations ApplicableSpecs() => Specializations.Druid;

        protected override bool IsAllowed(Item item, Specializations spec) => spec == Specializations.RestoDruid;
    }
}
