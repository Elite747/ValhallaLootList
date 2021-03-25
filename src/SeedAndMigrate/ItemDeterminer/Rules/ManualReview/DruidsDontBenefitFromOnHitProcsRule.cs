// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview
{
    internal class DruidsDontBenefitFromOnHitProcsRule : SimpleRule
    {
        protected override string DisallowReason => "Feral druids do not benefit from on-hit procs on weapons.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

        protected override bool AppliesTo(Item item)
        {
            if (item.Id == 32500u) // Crystal Spire of Karabor is a weapon with a proc that isn't on-hit
            {
                return false;
            }

            return item.HasProc && (item.Slot is InventorySlot.MainHand or InventorySlot.OffHand or InventorySlot.OneHand or InventorySlot.TwoHand);
        }

        protected override Specializations ApplicableSpecs() => Specializations.BearDruid | Specializations.CatDruid;

        protected override bool IsAllowed(Item item, Specializations spec) => false;
    }
}
