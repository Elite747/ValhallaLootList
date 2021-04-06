// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class SetLootListStatusDto
    {
        public SetLootListStatusDto()
        {
        }

        public SetLootListStatusDto(LootListStatus status, byte[] timestamp, long? submitTo = null)
        {
            Status = status;
            Timestamp = timestamp;
            SubmitTo = submitTo;
        }

        public LootListStatus Status { get; set; }

        public long? SubmitTo { get; set; }

        public byte[] Timestamp { get; set; } = Array.Empty<byte>();
    }
}
