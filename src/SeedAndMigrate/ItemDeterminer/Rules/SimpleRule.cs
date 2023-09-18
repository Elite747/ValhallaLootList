// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules;

internal abstract class SimpleRule : Rule
{
    protected sealed override ItemDetermination MakeDetermination(Item item, Specializations spec)
    {
        if (!IsAllowed(item, spec))
        {
            return new ItemDetermination(spec, DisallowLevel, DisallowReason);
        }
        return new ItemDetermination(spec, DeterminationLevel.Allowed, string.Empty);
    }

    protected abstract string DisallowReason { get; }

    protected abstract DeterminationLevel DisallowLevel { get; }

    protected abstract bool IsAllowed(Item item, Specializations spec);
}
