// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class MultiTimestampDto
{
    private Dictionary<byte, byte[]>? _timestamps;

    public Dictionary<byte, byte[]> Timestamps
    {
        get => _timestamps ??= [];
        set => _timestamps = value;
    }

    public byte Size { get; set; }
}
