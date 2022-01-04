// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class ScheduleSubmissionDto
{
    [Required]
    public DayOfWeek? Day { get; set; }

    [Required]
    public TimeSpan? StartTime { get; set; }

    [Required, Range(1.0, 10.0)]
    public double? Duration { get; set; }
}
