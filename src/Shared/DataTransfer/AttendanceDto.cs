// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class AttendanceDto
    {
        public CharacterDto? Character { get; set; }

        public bool IgnoreAttendance { get; set; }

        public string? IgnoreReason { get; set; }

        public Specializations? MainSpec { get; set; }
    }
}
