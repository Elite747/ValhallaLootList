// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer;

internal class AutoSpecializationDeterminer
{
    private readonly List<IDeterminationRule> _rules;

    public AutoSpecializationDeterminer()
    {
        _rules = [];

        foreach (var type in GetType().Assembly.DefinedTypes.Where(type => !type.IsAbstract && type.ImplementedInterfaces.Contains(typeof(IDeterminationRule))))
        {
            IDeterminationRule? rule = (IDeterminationRule?)Activator.CreateInstance(type);

            if (rule is not null)
            {
                _rules.Add(rule);
            }
        }
    }

    public IEnumerable<ItemDetermination> GetDeterminations(Item item)
    {
        foreach (var rule in _rules)
        {
            foreach (var determination in rule.GetDeterminations(item))
            {
                yield return determination;
            }
        }
    }

    public Specializations GetAllowedSpecs(Item item, bool includeManualReview)
    {
        var specs = SpecializationGroups.All;

        foreach (var rule in _rules)
        {
            foreach (var determination in rule.GetDeterminations(item))
            {
                switch (determination.Level)
                {
                    case DeterminationLevel.Allowed:
                        break;
                    case DeterminationLevel.ManualReview:
                        if (!includeManualReview)
                        {
                            specs &= ~determination.Specialization;
                        }
                        break;
                    case DeterminationLevel.Disallowed:
                    case DeterminationLevel.Unequippable:
                        specs &= ~determination.Specialization;
                        break;
                    default: throw new Exception("Determination rule returned an invalid DeterminationLevel.");
                }
            }
        }

        return specs;
    }

    public IEnumerable<string> GetDisallowedReasons(Item item, Specializations spec, bool excludeManualReview)
    {
        foreach (var rule in _rules)
        {
            foreach (var determination in rule.GetDeterminations(item))
            {
                if (determination.Specialization == spec && determination.Level != DeterminationLevel.Allowed)
                {
                    if (determination.Level == DeterminationLevel.ManualReview && excludeManualReview)
                    {
                        continue;
                    }

                    yield return determination.Reason;
                }
            }
        }
    }
}
