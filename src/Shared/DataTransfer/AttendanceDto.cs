// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class AttendanceDto
{
    private CharacterDto? _character;

    public CharacterDto Character
    {
        get => _character ?? throw new InvalidOperationException("Characer has not been set.");
        set => _character = value;
    }

    public bool IgnoreAttendance { get; set; }

    [StringLength(256)]
    public string? IgnoreReason { get; set; }

    public bool Standby { get; set; }

    public bool Disenchanter { get; set; }

    public Specializations MainSpec { get; set; }
}
