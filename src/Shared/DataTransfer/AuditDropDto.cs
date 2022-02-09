// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class AuditDropDto
{
    public uint ItemId { get; init; }

    public string ItemName { get; init; } = string.Empty;

    public long RaidId { get; init; }

    public DateTimeOffset RaidDate { get; init; }

    public long DropId { get; init; }

    public string TeamName { get; init; } = string.Empty;
}
