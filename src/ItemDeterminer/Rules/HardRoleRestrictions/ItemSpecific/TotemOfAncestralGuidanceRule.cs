// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer.Rules.HardRoleRestrictions.ItemSpecific
{
    internal class TotemOfAncestralGuidanceRule : SimpleRule
    {
        protected override string DisallowReason => "Totem of Ancestral Guidance increases damage to spells only used by the Elemental spec.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 32330u;

        protected override Specializations ApplicableSpecs() => Specializations.EnhanceShaman | Specializations.RestoShaman;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
