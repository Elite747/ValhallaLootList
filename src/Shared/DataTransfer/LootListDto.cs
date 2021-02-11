// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class LootListDto
    {
        private List<LootListEntryDto>? _entries;

        public string? CharacterId { get; set; }

        public string? CharacterName { get; set; }

        public Specializations MainSpec { get; set; }

        public Specializations OffSpec { get; set; }

        public byte Phase { get; set; }

        public bool Locked { get; set; }

        public string? ApprovedBy { get; set; }

        public List<LootListEntryDto> Entries
        {
            get => _entries ??= new List<LootListEntryDto>();
            set => _entries = value;
        }
    }
}
