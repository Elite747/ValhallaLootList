// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules.Disallowed.DeadStats
{
    internal abstract class DeadStatRule : SimpleRule
    {
        protected override DeterminationLevel DisallowLevel => DeterminationLevel.Disallowed;

        protected override string DisallowReason => $"The {GetStatDisplayName()} stat is not appropriate for the selected specialization.";

        protected abstract int GetStat(Item item);

        protected override sealed bool AppliesTo(Item item) => GetStat(item) > 0;

        protected override abstract Specializations ApplicableSpecs();

        protected override bool IsAllowed(Item item, Specializations spec) => false;

        protected abstract string GetStatDisplayName();
    }
}
