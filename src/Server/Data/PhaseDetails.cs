// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class PhaseDetails(byte id, DateTimeOffset startsAt)
{
    public byte Id { get; } = id;

    public DateTimeOffset StartsAt { get; set; } = startsAt;
}
