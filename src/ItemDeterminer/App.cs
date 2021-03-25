// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdGen;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ValhallaLootList.ItemDeterminer.Rules;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemDeterminer
{
    internal class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly ApplicationDbContext _appContext;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly List<IDeterminationRule> _rules;
        private readonly IIdGenerator<long> _idGenerator = new IdGenerator(0);

        public App(ILogger<App> logger, ApplicationDbContext appContext, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _appContext = appContext;
            _hostApplicationLifetime = hostApplicationLifetime;
            _rules = new List<IDeterminationRule>();

            foreach (var type in GetType().Assembly.DefinedTypes.Where(type => !type.IsAbstract && type.ImplementedInterfaces.Contains(typeof(IDeterminationRule))))
            {
                _rules.Add((IDeterminationRule)Activator.CreateInstance(type)!);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App started");

            var restrictions = await _appContext.ItemRestrictions.ToListAsync(cancellationToken);

            var restrictionsToKeep = new HashSet<long>();

            await foreach (var item in _appContext.Items.AsAsyncEnumerable().WithCancellation(cancellationToken))
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
                        var existingRestriction = restrictions.Find(r => r.ItemId == item.Id &&
                                                                         r.Reason == restriction.Reason &&
                                                                         r.Specializations == restriction.Specializations &&
                                                                         r.RestrictionLevel == restriction.RestrictionLevel);

                        if (existingRestriction is null)
                        {
                            _appContext.ItemRestrictions.Add(restriction);
                            _logger.LogInformation($"Added '{restriction.Reason}' to {item.Name} ({item.Id})");
                        }
                        else
                        {
                            restrictionsToKeep.Add(existingRestriction.Id);
                        }
                    }
                }
            }

            foreach (var restriction in restrictions.Where(r => r.Automated && !restrictionsToKeep.Contains(r.Id)))
            {
                _appContext.ItemRestrictions.Remove(restriction);
                _logger.LogInformation($"Removed '{restriction.Reason}' from {restriction.ItemId}.");
            }

            var changes = await _appContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Application context saved with {changes} changes.");

            _logger.LogInformation("App finished");
            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App stopped");

            return Task.CompletedTask;
        }
    }
}