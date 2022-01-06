// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdGen;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.SeedAndMigrate.ItemDeterminer.Rules;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.ItemDeterminer;

internal class ItemDeterminerStep
{
    private readonly ILogger<ItemDeterminerStep> _logger;
    private readonly ApplicationDbContext _context;
    private readonly List<IDeterminationRule> _rules;
    private readonly IIdGenerator<long> _idGenerator = new IdGenerator(0);

    public ItemDeterminerStep(ILogger<ItemDeterminerStep> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
        _rules = new List<IDeterminationRule>();

        foreach (var type in GetType().Assembly.DefinedTypes.Where(type => !type.IsAbstract && type.ImplementedInterfaces.Contains(typeof(IDeterminationRule))))
        {
            _rules.Add((IDeterminationRule)Activator.CreateInstance(type)!);
        }
    }

    public async Task DetermineAllItemsAsync(CancellationToken cancellationToken = default)
    {
        var restrictions = await _context.ItemRestrictions.ToListAsync(cancellationToken);

        var restrictionsToKeep = new HashSet<long>();

        await foreach (var item in _context.Items.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            foreach (var rule in _rules)
            {
                foreach (var restriction in rule.GetDeterminations(item)
                    .Where(d => d.Level != DeterminationLevel.Allowed)
                    .GroupBy(d => new { d.Reason, d.Level })
                    .Select(g => new ItemRestriction(_idGenerator.CreateId())
                    {
                        Automated = true,
                        Item = item,
                        ItemId = item.Id,
                        Reason = g.Key.Reason,
                        RestrictionLevel = g.Key.Level switch
                        {
                            DeterminationLevel.ManualReview => ItemRestrictionLevel.ManualReview,
                            DeterminationLevel.Disallowed => ItemRestrictionLevel.Restricted,
                            DeterminationLevel.Unequippable => ItemRestrictionLevel.Unequippable,
                            _ => throw new Exception("Unexpected determination level was returned by a rule.")
                        },
                        Specializations = g.Select(d => d.Specialization).Aggregate((l, r) => l | r)
                    }))
                {
                    var existingRestriction = restrictions.Find(r => r.ItemId == item.Id && r.Reason == restriction.Reason);

                    if (existingRestriction is null)
                    {
                        _context.ItemRestrictions.Add(restriction);
                        _logger.LogInformation("Added '{Restriction}' to {Item} ({ItemId})", restriction.Reason, item.Name, item.Id);
                    }
                    else
                    {
                        restrictionsToKeep.Add(existingRestriction.Id);

                        if (restriction.Specializations != existingRestriction.Specializations)
                        {
                            existingRestriction.Specializations = restriction.Specializations;
                            _logger.LogInformation("Updated specs for '{Restriction}' on {Item} ({ItemId})", restriction.Reason, item.Name, item.Id);
                        }

                        if (restriction.RestrictionLevel != existingRestriction.RestrictionLevel)
                        {
                            existingRestriction.RestrictionLevel = restriction.RestrictionLevel;
                            _logger.LogInformation("Updated restriction level for '{Restriction}' on {Item} ({ItemId})", restriction.Reason, item.Name, item.Id);
                        }
                    }
                }
            }
        }

        foreach (var restriction in restrictions.Where(r => r.Automated && !restrictionsToKeep.Contains(r.Id)))
        {
            _context.ItemRestrictions.Remove(restriction);
            _logger.LogInformation("Removed '{Restriction}' from {ItemId}.", restriction.Reason, restriction.ItemId);
        }

        var changes = await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Application context saved with {ChangeCount} changes.", changes);
    }
}
