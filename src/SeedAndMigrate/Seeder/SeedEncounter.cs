// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.SeedAndMigrate.Seeder;

public class SeedEncounter
{
    private List<uint>? _items10, _items25, _items10H, _items25H;

    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public sbyte Index { get; set; }

    public byte? Phase { get; set; }

    public List<uint> Items10
    {
        get => _items10 ??= [];
        set => _items10 = value;
    }

    public List<uint> Items10H
    {
        get => _items10H ??= [];
        set => _items10H = value;
    }

    public List<uint> Items25
    {
        get => _items25 ??= [];
        set => _items25 = value;
    }

    public List<uint> Items25H
    {
        get => _items25H ??= [];
        set => _items25H = value;
    }
}
