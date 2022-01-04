// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Server.Data;

public class RaidTeamSchedule
{
    public RaidTeamSchedule(long id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), "Id cannot be less than or equal to zero.");
        }

        Id = id;
    }

    public long Id { get; }

    [Required]
    public virtual RaidTeam RaidTeam { get; init; } = null!;

    public DayOfWeek Day { get; set; }

    public TimeSpan RealmTimeStart { get; set; }

    public TimeSpan Duration { get; set; }
}
