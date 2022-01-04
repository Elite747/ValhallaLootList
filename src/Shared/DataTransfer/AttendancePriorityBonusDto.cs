// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class AttendancePriorityBonusDto : PriorityBonusDto
{
    public int Attended { get; set; }

    public int AttendancePerPoint { get; set; }

    public int ObservedAttendances { get; set; }
}
