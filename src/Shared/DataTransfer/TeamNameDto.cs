// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public record TeamNameDto
    {
        private List<ScheduleDto>? _schedules;

        public long Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool Inactive { get; set; }

        public List<ScheduleDto> Schedules
        {
            get => _schedules ??= new();
            set => _schedules = value;
        }
    }
}
