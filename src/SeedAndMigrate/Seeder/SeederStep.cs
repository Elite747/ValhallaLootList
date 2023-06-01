// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate.Seeder;

internal class SeederStep
{
    private readonly ILogger<SeederStep> _logger;
    private readonly ApplicationDbContext _context;
    private readonly Config _config;

    public SeederStep(ILogger<SeederStep> logger, IOptions<Config> config, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
        _config = config.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        var seedInstances = await LoadSeedInstancesAsync(cancellationToken);
        var seedItems = await LoadSeedItemsAsync(cancellationToken);

        var existingItems = await _context.Items.ToDictionaryAsync(item => item.Id, cancellationToken);
        var existingInstances = await _context.Instances.ToDictionaryAsync(instance => instance.Id, cancellationToken);
        var existingEncounters = await _context.Encounters.ToDictionaryAsync(encounter => encounter.Id, cancellationToken);
        var existingEncounterItems = await _context.EncounterItems.ToListAsync(cancellationToken);

        foreach (var seedItem in seedItems)
        {
            if (!existingItems.TryGetValue(seedItem.Id, out var item))
            {
                item = new Item(seedItem.Id);
                _context.Items.Add(item);
                existingItems.Add(item.Id, item);
            }

            item.Agility = seedItem.Agility;
            item.ArmorPenetration = seedItem.ArmorPenetration;
            item.BlockRating = seedItem.BlockRating;
            item.BlockValue = seedItem.BlockValue;
            item.Defense = seedItem.Defense;
            item.Dodge = seedItem.Dodge;
            item.Expertise = seedItem.Expertise;
            item.HasOnUse = seedItem.HasOnUse;
            item.HasProc = seedItem.HasProc;
            item.HasSpecial = seedItem.HasSpecial;
            item.Haste = seedItem.Haste;
            item.Intellect = seedItem.Intellect;
            item.ItemLevel = seedItem.ItemLevel;
            item.ManaPer5 = seedItem.ManaPer5;
            item.AttackPower = seedItem.AttackPower;
            item.Crit = seedItem.Crit;
            item.Name = seedItem.Name;
            item.Parry = seedItem.Parry;
            item.Hit = seedItem.Hit;
            item.RewardFromId = seedItem.RewardFromId;
            item.Slot = seedItem.Slot;
            item.SpellPenetration = seedItem.SpellPenetration;
            item.SpellPower = seedItem.SpellPower;
            item.Spirit = seedItem.Spirit;
            item.Stamina = seedItem.Stamina;
            item.Strength = seedItem.Strength;
            item.Type = seedItem.Type;
            item.UsableClasses = seedItem.UsableClasses;
            item.IsUnique = seedItem.IsUnique;
            item.QuestId = seedItem.QuestId;
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
                encounter.Phase = seedInstance.Phase ?? seedEncounter.Phase ?? throw new Exception("Instance or encounter needs a phase set.");

                var encounterItems = new List<(uint itemId, byte size, bool heroic)>();

                foreach (var itemId in seedEncounter.Items10)
                {
                    encounterItems.Add((itemId, 10, false));
                }
                foreach (var itemId in seedEncounter.Items10H)
                {
                    encounterItems.Add((itemId, 10, true));
                }
                foreach (var itemId in seedEncounter.Items25)
                {
                    encounterItems.Add((itemId, 25, false));
                }
                foreach (var itemId in seedEncounter.Items25H)
                {
                    encounterItems.Add((itemId, 25, true));
                }

                foreach ((uint itemId, byte size, bool heroic) in encounterItems)
                {
                    if (existingItems.TryGetValue(itemId, out var item))
                    {
                        var is25 = size == 25;
                        item.Phase = encounter.Phase;

                        if (existingEncounterItems.Find(ei => ei.ItemId == itemId && ei.EncounterId == encounter.Id && ei.Is25 == is25 && ei.Heroic == heroic) is { } existing)
                        {
                            existingEncounterItems.Remove(existing);
                        }
                        else
                        {
                            _context.EncounterItems.Add(new() { Encounter = encounter, EncounterId = encounter.Id, Item = item, ItemId = itemId, Is25 = is25, Heroic = heroic });
                        }

                        foreach (var sourceItem in existingItems.Values.Where(item2 => item2.RewardFromId == itemId))
                        {
                            sourceItem.Phase = encounter.Phase;
                        }
                    }
                }
            }
        }

        _context.EncounterItems.RemoveRange(existingEncounterItems);

        int changes = await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Application context saved with {ChangeCount} changes.", changes);
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
