// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class TeamMember
{
    public DateTimeOffset JoinedAt { get; set; }

    public long CharacterId { get; set; }

    public long TeamId { get; set; }

    public bool Enchanted { get; set; }

    public bool Prepared { get; set; }

    public bool Disenchanter { get; set; }

    public virtual Character? Character { get; set; }

    public virtual RaidTeam? Team { get; set; }
}
