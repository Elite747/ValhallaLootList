// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data
{
    public class RaidTeamLeader
    {
        public long RaidTeamId { get; set; }

        public long UserId { get; set; }

        public virtual RaidTeam RaidTeam { get; set; } = null!;

        public virtual AppUser User { get; set; } = null!;
    }
}
