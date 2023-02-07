// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data.Import;

public class ImportEncounter
{
    public ImportEncounter(EncounterDto? encounter, IEnumerable<uint>? items)
    {
        Encounter = encounter;
        Items = new();

        if (items is not null)
        {
            Items.AddRange(items);
        }
    }

    public EncounterDto? Encounter { get; }

    public List<uint> Items { get; }

    public static IEnumerable<ImportEncounter> CreateFromItems(IEnumerable<uint> items, IList<InstanceDto> instances)
    {
        var candidateEncounters = new List<ImportEncounter>();

        var unknownItems = items.ToHashSet();

        foreach (var instance in instances)
        {
            foreach (var encounter in instance.Encounters)
            {
                var encounterItems = new List<uint>();
                foreach (var variant in encounter.Variants)
                {
                    if (variant.Items.Any(items.Contains))
                    {
                        unknownItems.ExceptWith(variant.Items);
                        encounterItems.AddRange(items.Where(variant.Items.Contains));
                    }
                }
                if (encounterItems.Count > 0)
                {
                    candidateEncounters.Add(new ImportEncounter(encounter, encounterItems));
                }
            }
        }

        if (unknownItems.Count != 0)
        {
            candidateEncounters.Add(new ImportEncounter(null, items.Where(unknownItems.Contains)));
        }

        var completeCandidates = candidateEncounters.Where(e => items.All(e.Items.Contains)).ToList();

        if (completeCandidates.Count > 0)
        {
            return completeCandidates;
        }

        return candidateEncounters;
    }
}
