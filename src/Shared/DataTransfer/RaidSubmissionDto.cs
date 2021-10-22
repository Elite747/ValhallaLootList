// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class RaidSubmissionDto : IValidatableObject
    {
        private List<long>? _attendees, _rto;

        [Required]
        public long? TeamId { get; set; }

        [Required]
        public int Phase { get; set; }

        public List<long> Attendees
        {
            get => _attendees ??= new();
            set => _attendees = value;
        }

        public List<long> Rto
        {
            get => _rto ?? new();
            set => _rto = value;
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
