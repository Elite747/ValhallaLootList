// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList.Server.Data
{
    public class Raid : KeyedRow
    {
        public DateTime StartedAtUtc { get; set; }

        public virtual RaidTeamSchedule Schedule { get; set; } = null!;

        public virtual ICollection<RaidAttendee> Attendees { get; set; } = new HashSet<RaidAttendee>();
    }
}
