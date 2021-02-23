// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class EncounterDropDto
    {
        public uint ItemId { get; set; }

        public string? ItemName { get; set; }

        public string? WinnerId { get; set; }

        public string? WinnerName { get; set; }

        public string? AwardedBy { get; set; }

        public DateTimeOffset AwardedAt { get; set; }
    }
}
