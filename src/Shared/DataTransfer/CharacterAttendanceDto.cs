// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class CharacterAttendanceDto
    {
        public string RaidId { get; set; } = string.Empty;

        public DateTimeOffset StartedAt { get; set; }

        public string TeamId { get; set; } = string.Empty;

        public string TeamName { get; set; } = string.Empty;

        public bool IgnoreAttendance { get; set; }

        public string? IgnoreReason { get; set; }
    }
}
