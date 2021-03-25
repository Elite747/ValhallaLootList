// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class Character
    {
        public Character(long id)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
            }

            Id = id;
        }

        public long Id { get; }

        [Required, StringLength(16, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        public PlayerRace Race { get; set; }

        public Classes Class { get; set; }

        public bool IsFemale { get; set; }

        public long? VerifiedById { get; set; }

        public RaidMemberStatus MemberStatus { get; set; }

        public long? TeamId { get; set; }

        public virtual RaidTeam? Team { get; set; }

        public virtual ICollection<RaidAttendee> Attendances { get; set; } = new HashSet<RaidAttendee>();

        public virtual ICollection<DropPass> Passes { get; set; } = new HashSet<DropPass>();

        public virtual ICollection<CharacterLootList> CharacterLootLists { get; set; } = new HashSet<CharacterLootList>();

        public virtual ICollection<CharacterEncounterKill> EncounterKills { get; set; } = new HashSet<CharacterEncounterKill>();
    }
}
