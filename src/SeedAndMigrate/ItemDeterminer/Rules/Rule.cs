// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules;

internal abstract class Rule : IDeterminationRule
{
    public IEnumerable<ItemDetermination> GetDeterminations(Item item)
    {
        if (AppliesTo(item))
        {
            return EnumerateDeterminations(item);
        }

        return Array.Empty<ItemDetermination>();
    }

    private IEnumerable<ItemDetermination> EnumerateDeterminations(Item item)
    {
        for (int i = 1; Enum.IsDefined(typeof(Specializations), i); i <<= 1)
        {
            var spec = (Specializations)i;

            if ((spec & ApplicableSpecs()) == spec)
            {
                yield return MakeDetermination(item, spec);
            }
        }
    }

    protected virtual bool AppliesTo(Item item) => true;

    protected virtual Specializations ApplicableSpecs() => SpecializationGroups.All;

    protected abstract ItemDetermination MakeDetermination(Item item, Specializations spec);
}
