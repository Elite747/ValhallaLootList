// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class KillSubmissionDto
    {
        private List<string>? _characters;
        private List<uint>? _drops;

        [Required]
        public string? EncounterId { get; set; }

        public List<string> Characters
        {
            get => _characters ??= new();
            set => _characters = value;
        }

        public List<uint> Drops
        {
            get => _drops ??= new();
            set => _drops = value;
        }
    }
}
