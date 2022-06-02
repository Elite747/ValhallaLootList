// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class EncounterItem
{
    public virtual Encounter Encounter { get; set; } = null!;

    public string EncounterId { get; set; } = null!;

    public virtual Item Item { get; set; } = null!;

    public uint ItemId { get; set; }
}
