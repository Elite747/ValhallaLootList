// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class WonDropDto
{
    public long CharacterId { get; set; }

    public uint ItemId { get; set; }

    public DateTimeOffset AwardedAt { get; set; }

    public long RaidId { get; set; }
}
