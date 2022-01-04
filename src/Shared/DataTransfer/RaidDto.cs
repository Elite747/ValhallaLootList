// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class RaidDto
{
    private List<AttendanceDto>? _attendees;
    private List<EncounterKillDto>? _kills;

    public long Id { get; set; }

    public long TeamId { get; set; }

    public string TeamName { get; set; } = string.Empty;

    public int Phase { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset LocksAt { get; set; }

    public List<AttendanceDto> Attendees
    {
        get => _attendees ??= new();
        set => _attendees = value;
    }

    public List<EncounterKillDto> Kills
    {
        get => _kills ??= new();
        set => _kills = value;
    }
}
