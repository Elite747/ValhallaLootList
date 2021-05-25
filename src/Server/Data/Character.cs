// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValhallaLootList.Helpers;

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

        [Required, StringLength(12, MinimumLength = 2), CharacterName]
        public string Name { get; set; } = string.Empty;

        public PlayerRace Race { get; set; }

        public Classes Class { get; set; }

        public bool IsFemale { get; set; }

        public bool Deactivated { get; set; }

        public long? VerifiedById { get; set; }

        public long? OwnerId { get; set; }

        public RaidMemberStatus MemberStatus { get; set; }

        public DateTimeOffset JoinedTeamAt { get; set; }

        public long? TeamId { get; set; }

        public virtual RaidTeam? Team { get; set; }

        public virtual AppUser? Owner { get; set; }

        public virtual ICollection<RaidAttendee> Attendances { get; set; } = new HashSet<RaidAttendee>();

        public virtual ICollection<CharacterLootList> CharacterLootLists { get; set; } = new HashSet<CharacterLootList>();

        public virtual ICollection<CharacterEncounterKill> EncounterKills { get; set; } = new HashSet<CharacterEncounterKill>();

        public virtual ICollection<Donation> Donations { get; set; } = new HashSet<Donation>();

        public virtual ICollection<TeamRemoval> Removals { get; set; } = new HashSet<TeamRemoval>();
    }
}
