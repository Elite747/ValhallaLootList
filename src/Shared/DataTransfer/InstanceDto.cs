// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class InstanceDto
    {
        private List<EncounterDto>? _encounters;

        public string? Id { get; set; }

        public string? Name { get; set; }

        public byte Phase { get; set; }

        public List<EncounterDto> Encounters
        {
            get => _encounters ??= new();
            set => _encounters = value;
        }
    }
}
