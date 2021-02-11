// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class CharacterDto
    {
        public string? Id { get; set; }

        public string? Name { get; set; }

        public PlayerRace Race { get; set; }

        public Classes Class { get; set; }

        public Gender Gender { get; set; }

        public string? TeamId { get; set; }

        public string? TeamName { get; set; }

        public bool Editable { get; set; }
    }
}
