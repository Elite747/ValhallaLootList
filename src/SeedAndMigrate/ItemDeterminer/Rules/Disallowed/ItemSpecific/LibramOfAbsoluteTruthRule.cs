// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.ItemSpecific
{
    internal class LibramOfAbsoluteTruthRule : SimpleRule
    {
        protected override string DisallowReason => "Libram of Absolute Truth is only appropriate for the Holy spec.";

        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override bool AppliesTo(Item item) => item.Id == 30063u;

        protected override Specializations ApplicableSpecs() => SpecializationGroups.Paladin;

        protected override bool IsAllowed(Item item, Specializations spec) => spec == Specializations.HolyPaladin;
    }
}
