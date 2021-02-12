// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Raid : KeyedRow
    {
        public DateTime StartedAtUtc { get; set; }

        [Required]
        public string InstanceId { get; set; } = string.Empty;

        [Required]
        public string RaidTeamId { get; set; } = string.Empty;

        [Required]
        public virtual Instance Instance { get; set; } = null!;

        [Required]
        public virtual RaidTeam RaidTeam { get; set; } = null!;

        public virtual ICollection<RaidAttendee> Attendees { get; set; } = new HashSet<RaidAttendee>();

        public virtual ICollection<EncounterKill> Kills { get; set; } = new HashSet<EncounterKill>();
    }
}
