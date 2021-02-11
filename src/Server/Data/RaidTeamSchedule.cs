﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class RaidTeamSchedule : KeyedRow
    {
        [Required]
        public virtual RaidTeam RaidTeam { get; init; } = null!;

        public DayOfWeek Day { get; set; }

        public TimeSpan RealmTimeStart { get; set; }

        public TimeSpan Duration { get; set; }

        public virtual ICollection<Raid> Raids { get; set; } = new HashSet<Raid>();
    }
}
