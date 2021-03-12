// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer
{
    public class GuildMemberDto
    {
        public long Id { get; set; }

        public string? Nickname { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Discriminator { get; set; } = string.Empty;
    }
}
