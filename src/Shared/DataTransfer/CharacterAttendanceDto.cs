// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class CharacterAttendanceDto
    {
        public long RaidId { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public long TeamId { get; set; }

        public string TeamName { get; set; } = string.Empty;

        public bool IgnoreAttendance { get; set; }

        public string? IgnoreReason { get; set; }
    }
}
