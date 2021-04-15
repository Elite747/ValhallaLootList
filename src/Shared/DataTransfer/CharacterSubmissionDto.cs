// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.DataTransfer
{
    public class CharacterSubmissionDto
    {
        [Required, CharacterName]
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
