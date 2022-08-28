// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class EncounterDto
{
    private IList<EncounterVariant>? _items;

    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsTrash { get; set; }

    public IList<EncounterVariant> Variants
    {
        get => _items ??= new List<EncounterVariant>();
        set => _items = value;
    }
}

public class EncounterVariant
{
    private IList<uint>? _items;

    public IList<uint> Items
    {
        get => _items ??= new List<uint>();
        set => _items = value;
    }

    public int Size { get; set; }

    public bool Heroic { get; set; }
}
