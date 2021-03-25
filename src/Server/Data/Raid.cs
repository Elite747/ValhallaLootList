// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Raid
    {
        public Raid(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
            }

            Id = id;
        }

        public long Id { get; }

        public DateTimeOffset StartedAt { get; set; }

        public byte Phase { get; set; }

        [Required]
        public long RaidTeamId { get; set; }

        [Required]
        public virtual RaidTeam RaidTeam { get; set; } = null!;

        public virtual ICollection<RaidAttendee> Attendees { get; set; } = new HashSet<RaidAttendee>();

        public virtual ICollection<EncounterKill> Kills { get; set; } = new HashSet<EncounterKill>();
    }
}
