// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class AttendeeSubmissionDto
{
    [Required]
    public long? CharacterId { get; set; }

    public bool Standby { get; set; }
}
