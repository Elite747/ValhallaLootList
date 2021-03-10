// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class RaidDto
    {
        private List<CharacterDto>? _attendees;
        private List<EncounterKillDto>? _kills;

        public string Id { get; set; } = string.Empty;

        public string TeamId { get; set; } = string.Empty;

        public string TeamName { get; set; } = string.Empty;

        public int Phase { get; set; }

        public DateTimeOffset StartedAt { get; set; }

        public List<CharacterDto> Attendees
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
