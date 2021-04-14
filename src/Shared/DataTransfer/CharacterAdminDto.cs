// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class CharacterAdminDto
    {
        private List<TeamRemovalDto>? _teamRemovals;

        public List<TeamRemovalDto> TeamRemovals
        {
            get => _teamRemovals ??= new();
            set => _teamRemovals = value;
        }

        public GuildMemberDto? Owner { get; set; }

        public GuildMemberDto? VerifiedBy { get; set; }
    }
}
