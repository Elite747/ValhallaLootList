// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class TimestampDto
{
    public byte[] Timestamp { get; set; } = [];
}

public class LootListActionDto : TimestampDto
{
    public byte Phase { get; set; }

    public byte Size { get; set; }
}
