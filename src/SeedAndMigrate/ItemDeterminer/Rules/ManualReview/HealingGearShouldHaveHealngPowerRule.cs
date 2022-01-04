// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview;

internal class HealingGearShouldHaveHealngPowerRule : SimpleRule
{
    protected override string DisallowReason => "Healing gear should have healing power.";
    protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.Healer;

    protected override bool AppliesTo(Item item) => item.SpellPower > 0 && item.HealingPower == item.SpellPower;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
