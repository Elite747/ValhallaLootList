// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class MemberDto
    {
        private CharacterDto? _character;
        private List<MemberLootListDto>? _lootLists;

        public CharacterDto Character
        {
            get => _character ?? throw new InvalidOperationException("Characer has not been set.");
            set => _character = value;
        }

        public List<MemberLootListDto> LootLists
        {
            get => _lootLists ??= new();
            set => _lootLists = value;
        }

        public RaidMemberStatus? Status { get; set; }

        public bool? Verified { get; set; }

        public long DonatedThisMonth { get; set; }

        public long DonatedNextMonth { get; set; }

        public long ThisMonthRequiredDonations { get; set; }

        public long NextMonthRequiredDonations { get; set; }

        //TODO: Attendance info
    }

    public class DonationSubmissionDto
    {
        public long CharacterId { get; set; }

        public int CopperAmount { get; set; }
    }
}
