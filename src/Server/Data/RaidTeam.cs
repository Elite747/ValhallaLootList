// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.Server.Data
{
    public class RaidTeam
    {
        public RaidTeam(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
            }

            Id = id;
        }

        public long Id { get; }

        [Required, StringLength(24, MinimumLength = 2), GuildName]
        public string Name { get; set; } = string.Empty;

        public bool Inactive { get; set; }

        public virtual ICollection<Character> Roster { get; set; } = new HashSet<Character>();

        public virtual ICollection<RaidTeamSchedule> Schedules { get; set; } = new HashSet<RaidTeamSchedule>();

        public virtual ICollection<Raid> Raids { get; set; } = new HashSet<Raid>();

        public virtual ICollection<LootListTeamSubmission> Submissions { get; set; } = new HashSet<LootListTeamSubmission>();

        public virtual ICollection<TeamRemoval> Removals { get; set; } = new HashSet<TeamRemoval>();

        public virtual ICollection<RaidTeamLeader> Leaders { get; set; } = new HashSet<RaidTeamLeader>();
    }
}
