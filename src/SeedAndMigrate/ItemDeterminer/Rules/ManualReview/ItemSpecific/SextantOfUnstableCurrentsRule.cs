// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.ManualReview.ItemSpecific;

internal class SextantOfUnstableCurrentsRule : SimpleRule
{
    protected override string DisallowReason => "Sextant of Unstable Currents procs only on spell critical strikes, which is only usable by spellcasters.";

    protected override DeterminationLevel DisallowLevel => DeterminationLevel.ManualReview;

    protected override bool AppliesTo(Item item) => item.Id == 30626u;

    protected override Specializations ApplicableSpecs() => SpecializationGroups.All & ~SpecializationGroups.CasterDps;

    protected override bool IsAllowed(Item item, Specializations spec) => false;
}
