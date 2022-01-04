// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer;

public class UpdateAttendanceSubmissionDto : IValidatableObject
{
    public bool IgnoreAttendance { get; set; }

    public string? IgnoreReason { get; set; }

    public bool Rto { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IgnoreAttendance && string.IsNullOrWhiteSpace(IgnoreReason))
        {
            return new[] { new ValidationResult("Reason is required if ignoring attendance.", new[] { nameof(IgnoreReason) }) };
        }

        return Array.Empty<ValidationResult>();
    }
}
