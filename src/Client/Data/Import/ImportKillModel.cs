// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Import;

public class ImportKillModel
{
    public List<ImportCharacter>? Characters { get; set; }

    public List<uint>? Items { get; set; }

    public List<ImportDrop>? Drops { get; set; }

    public long? Timestamp { get; set; }
}

public class ImportDrop
{
    public uint ItemId { get; set; }

    public string? WinnerName { get; set; }

    public bool Disenchanted { get; set; }
}
