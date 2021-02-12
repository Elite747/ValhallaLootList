// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class RaidTeam : KeyedRow
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Character> Roster { get; set; } = new HashSet<Character>();

        public virtual ICollection<RaidTeamSchedule> Schedules { get; set; } = new HashSet<RaidTeamSchedule>();

        public virtual ICollection<Raid> Raids { get; set; } = new HashSet<Raid>();
    }
}
