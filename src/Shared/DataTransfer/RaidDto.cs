// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class RaidDto
    {
        private List<RaidAttendeeDto>? _attendees;
        private List<EncounterKillDto>? _kills;

        public string? Id { get; set; }

        public string? InstanceId { get; set; }

        public string? InstanceName { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public List<RaidAttendeeDto> Attendees
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
}
