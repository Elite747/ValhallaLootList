// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data.Import;

public sealed class ImportEncounter
{
    private ImportEncounter() : this([])
    {
    }

    private ImportEncounter(List<ImportDrop> drops)
    {
        Drops = drops;
    }

    public DateTimeOffset? Timestamp { get; private init; }

    public EncounterDto? Encounter { get; private init; }

    public List<ImportDrop> Drops { get; }

    public static IEnumerable<ImportEncounter> Create(ImportKillModel kill, IList<InstanceDto> instances)
    {
        DateTimeOffset? timestamp = kill.Timestamp.HasValue ? DateTimeOffset.FromUnixTimeSeconds(kill.Timestamp.Value) : null;
        var candidateEncounters = new List<ImportEncounter>();

        var allDrops = kill.Drops?.ToList() ?? [];

        if (kill.Items?.Count > 0)
        {
            allDrops.AddRange(kill.Items.Select(itemId => new ImportDrop { ItemId = itemId }));
        }

        var unknownItems = allDrops.Select(d => d.ItemId).ToHashSet();

        foreach (var instance in instances)
        {
            foreach (var encounter in instance.Encounters)
            {
                var allDropsCopy = allDrops.ToList();
                var encounterDrops = new List<ImportDrop>();
                foreach (var variant in encounter.Variants)
                {
                    if (variant.Items.Any(itemId => allDropsCopy.Any(drop => drop.ItemId == itemId)))
                    {
                        unknownItems.ExceptWith(variant.Items);
                        foreach (var drop in allDropsCopy.Where(d => variant.Items.Contains(d.ItemId)).ToList())
                        {
                            encounterDrops.Add(drop);
                            allDropsCopy.Remove(drop);
                        }
                    }
                }
                if (encounterDrops.Count > 0)
                {
                    candidateEncounters.Add(new ImportEncounter(encounterDrops)
                    {
                        Encounter = encounter,
                        Timestamp = timestamp
                    });
                }
            }
        }

        if (unknownItems.Count != 0)
        {
            candidateEncounters.Add(new ImportEncounter(allDrops.Where(drop => unknownItems.Contains(drop.ItemId)).ToList())
            {
                Timestamp = timestamp
            });
        }

        var completeCandidates = candidateEncounters.Where(e => allDrops.All(e.Drops.Contains)).ToList();

        if (completeCandidates.Count > 0)
        {
            return completeCandidates;
        }

        return candidateEncounters;
    }
}
