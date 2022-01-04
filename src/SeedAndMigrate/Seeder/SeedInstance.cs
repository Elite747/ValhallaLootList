// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.SeedAndMigrate.Seeder;

public class SeedInstance
{
    private List<SeedEncounter>? _encounters;

    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public byte Phase { get; set; }

    public List<SeedEncounter> Encounters
    {
        get => _encounters ??= new();
        set => _encounters = value;
    }
}
