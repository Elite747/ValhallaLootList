// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.DataTransfer
{
    public class ScheduleDto
    {
        public DayOfWeek Day { get; set; }

        public TimeSpan RealmTimeStart { get; set; }

        public TimeSpan Duration { get; set; }
    }
}
