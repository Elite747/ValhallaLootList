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
        var itemsHashSet = items.ToHashSet();

        foreach (var instance in instances)
        {
            foreach (var encounter in instance.Encounters)
            {
                if (encounter.Items.Any(itemsHashSet.Contains))
                {
                    itemsHashSet.ExceptWith(encounter.Items);
                    yield return new ImportEncounter(encounter, items.Where(encounter.Items.Contains));
                }
            }
        }

        if (itemsHashSet.Count != 0)
        {
            yield return new ImportEncounter(null, items.Where(itemsHashSet.Contains));
        }
    }
}
