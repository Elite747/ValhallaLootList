// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class RaidSubmissionDto : IValidatableObject
    {
        private List<string>? _attendees;

        [Required]
        public string? TeamId { get; set; }

        [Required]
        public int Phase { get; set; }

        public List<string> Attendees
        {
            get => _attendees ??= new();
            set => _attendees = value;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Attendees.Count == 0)
            {
                yield return new ValidationResult("At least one attendee must be present.", new[] { nameof(Attendees) });
            }
        }
    }
}
