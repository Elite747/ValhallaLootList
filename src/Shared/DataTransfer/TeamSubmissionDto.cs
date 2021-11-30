// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.DataTransfer
{
    public class TeamSubmissionDto
    {
        private List<ScheduleSubmissionDto>? _schedules;

        [Required, GuildName]
        public string? Name { get; set; }

        public bool Inactive { get; set; }

        public List<ScheduleSubmissionDto> Schedules
        {
            get => _schedules ??= new();
            set => _schedules = value;
        }
    }
}
