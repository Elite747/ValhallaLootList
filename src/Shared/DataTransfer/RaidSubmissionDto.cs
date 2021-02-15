// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class RaidSubmissionDto
    {
        private List<RaidSubmissionAttendeeDto>? _attendees;

        [Required]
        public string? TeamId { get; set; }

        [Required]
        public string? InstanceId { get; set; }

        public List<RaidSubmissionAttendeeDto> Attendees
        {
            get => _attendees ??= new();
            set => _attendees = value;
        }
    }
}
