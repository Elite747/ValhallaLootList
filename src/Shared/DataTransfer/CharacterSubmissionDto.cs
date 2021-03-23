// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.DataTransfer
{
    public class CharacterSubmissionDto
    {
        [Required, StringLength(16, MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        public PlayerRace? Race { get; set; }

        [Required]
        public Classes? Class { get; set; }

        [Required]
        public Gender? Gender { get; set; }

        public bool SenderIsOwner { get; set; } = true;
    }
}
