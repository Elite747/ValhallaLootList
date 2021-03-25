// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class EncounterDropDto
    {
        public long Id { get; set; }

        public uint ItemId { get; set; }

        public string? ItemName { get; set; }

        public long? WinnerId { get; set; }

        public string? WinnerName { get; set; }

        public long? AwardedBy { get; set; }

        public DateTimeOffset AwardedAt { get; set; }
    }
}
