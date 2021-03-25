// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Data.Seeding;

namespace ValhallaLootList.SeedAndMigrate
{
    internal class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly ApplicationDbContext _context;
        private readonly Config _config;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;

        public App(ILogger<App> logger, IOptions<Config> config, ApplicationDbContext context, IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _context = context;
            _config = config.Value;
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("App started");

            await _context.Database.MigrateAsync(cancellationToken);

            var seedInstances = await LoadSeedInstancesAsync(cancellationToken);
            var seedItems = await LoadSeedItemsAsync(cancellationToken);

            var existingItems = await _context.Items.ToDictionaryAsync(item => item.Id, cancellationToken);
            var existingInstances = await _context.Instances.ToDictionaryAsync(instance => instance.Id, cancellationToken);
            var existingEncounters = await _context.Encounters.ToDictionaryAsync(encounter => encounter.Id, cancellationToken);

            foreach (var seedItem in seedItems)
            {
                if (!existingItems.TryGetValue(seedItem.Id, out var item))
                {
                    item = new Item(seedItem.Id);
                    _context.Items.Add(item);
                    existingItems.Add(item.Id, item);
                }

                item.Agility = seedItem.Agility;
                item.Armor = seedItem.Armor;
                item.ArmorPenetration = seedItem.ArmorPenetration;
                item.BlockRating = seedItem.BlockRating;
                item.BlockValue = seedItem.BlockValue;
                item.Defense = seedItem.Defense;
                item.Dodge = seedItem.Dodge;
                item.DPS = seedItem.DPS;
                item.Expertise = seedItem.Expertise;
                item.HasOnUse = seedItem.HasOnUse;
                item.HasProc = seedItem.HasProc;
                item.HasSpecial = seedItem.HasSpecial;
                item.Haste = seedItem.Haste;
                item.HealingPower = seedItem.HealingPower;
                item.HealthPer5 = seedItem.HealthPer5;
                item.Intellect = seedItem.Intellect;
                item.ItemLevel = seedItem.ItemLevel;
                item.ManaPer5 = seedItem.ManaPer5;
                item.MeleeAttackPower = seedItem.MeleeAttackPower;
                item.MeleeCrit = seedItem.MeleeCrit;
                item.Name = seedItem.Name;
                item.Parry = seedItem.Parry;
                item.PhysicalHit = seedItem.PhysicalHit;
                item.RangedAttackPower = seedItem.RangedAttackPower;
                item.RangedCrit = seedItem.RangedCrit;
                item.Resilience = seedItem.Resilience;
                item.RewardFromId = seedItem.RewardFromId;
                item.Slot = seedItem.Slot;
                item.Sockets = seedItem.Sockets;
                item.Speed = seedItem.Speed;
                item.SpellCrit = seedItem.SpellCrit;
                item.SpellHaste = seedItem.SpellHaste;
                item.SpellHit = seedItem.SpellHit;
                item.SpellPenetration = seedItem.SpellPenetration;
                item.SpellPower = seedItem.SpellPower;
                item.Spirit = seedItem.Spirit;
                item.Stamina = seedItem.Stamina;
                item.Strength = seedItem.Strength;
                item.TopEndDamage = seedItem.TopEndDamage;
                item.Type = seedItem.Type;
                item.UsableClasses = seedItem.UsableClasses;
            }

            foreach (var item in existingItems.Values)
            {
                item.Encounter = null;
                item.EncounterId = null;
            }

            foreach (var seedInstance in seedInstances)
            {
                if (!existingInstances.TryGetValue(seedInstance.Id, out var instance))
                {
                    instance = new Instance(seedInstance.Id);
                    _context.Instances.Add(instance);
                }

                instance.Name = seedInstance.Name;
                instance.Phase = seedInstance.Phase;

                foreach (var seedEncounter in seedInstance.Encounters)
                {
                    if (!existingEncounters.TryGetValue(seedEncounter.Id, out var encounter))
                    {
                        encounter = new Encounter(seedEncounter.Id);
                        _context.Encounters.Add(encounter);
                    }

                    encounter.Index = seedEncounter.Index;
                    encounter.Instance = instance;
                    encounter.InstanceId = instance.Id;
                    encounter.Name = seedEncounter.Name;

                    foreach (var itemId in seedEncounter.Items)
                    {
                        var item = existingItems[itemId];
                        item.Encounter = encounter;
                        item.EncounterId = encounter.Id;
                        item.Phase = seedInstance.Phase;

                        foreach (var sourceItem in existingItems.Values.Where(item2 => item2.RewardFromId == itemId))
                        {
                            sourceItem.Phase = seedInstance.Phase;
                        }
                    }
                }
            }

            int changes = await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Saved {changes} changes to the database.");

            _logger.LogDebug("App finished");
            _hostApplicationLifetime.StopApplication();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("App stopped");

            return Task.CompletedTask;
        }

        private async Task<List<SeedInstance>> LoadSeedInstancesAsync(CancellationToken cancellationToken)
        {
            using var fs = File.OpenRead(_config.SeedInstancesPath);
            var instances = await JsonSerializer.DeserializeAsync<List<SeedInstance>>(fs, cancellationToken: cancellationToken);
            Debug.Assert(instances is not null);
            return instances;
        }

        private async Task<List<SeedItem>> LoadSeedItemsAsync(CancellationToken cancellationToken)
        {
            using var fs = File.OpenRead(_config.SeedItemsPath);
            var items = await JsonSerializer.DeserializeAsync<List<SeedItem>>(fs, cancellationToken: cancellationToken);
            Debug.Assert(items is not null);
            return items;
        }
    }
}