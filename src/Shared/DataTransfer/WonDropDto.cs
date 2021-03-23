// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class WonDropDto
    {
        public string CharacterId { get; set; } = string.Empty;

        public uint ItemId { get; set; }

        public DateTimeOffset AwardedAt { get; set; }

        public string RaidId { get; set; } = string.Empty;
    }
}
