// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data
{
    public class CharacterLootList
    {
        [Required]
        public long CharacterId { get; set; }

        [Required]
        public virtual Character Character { get; set; } = null!;

        public Specializations MainSpec { get; set; }

        public Specializations OffSpec { get; set; }

        public byte Phase { get; set; }

        [Obsolete("Use Status instead")]
        public bool Locked { get; set; }

        public LootListStatus Status { get; set; }

        public long? SubmittedToId { get; set; }

        public long? ApprovedBy { get; set; }

        public RaidTeam? SubmittedTo { get; set; }

        public virtual ICollection<LootListEntry> Entries { get; set; } = new HashSet<LootListEntry>();
    }
}
