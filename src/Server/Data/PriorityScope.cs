// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Server.Data
{
    public class PriorityScope
    {
        public long Id { get; set; }

        public DateTimeOffset StartsAt { get; set; }

        public int ObservedAttendances { get; set; }

        public int AttendancesPerPoint { get; set; }

        public int FullTrialPenalty { get; set; }

        public int HalfTrialPenalty { get; set; }

        public int RequiredDonationCopper { get; set; }
    }
}
