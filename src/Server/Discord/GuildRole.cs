// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Server.Discord
{
    public class GuildRole
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("color")]
        public int Color { get; set; }

        [JsonPropertyName("hoist")]
        public bool Hoist { get; set; }

        [JsonPropertyName("position")]
        public int Position { get; set; }

        [JsonPropertyName("permissions")]
        public string Permissions { get; set; } = string.Empty;

        [JsonPropertyName("managed")]
        public bool Managed { get; set; }

        [JsonPropertyName("mentionable")]
        public bool Mentionable { get; set; }

        [JsonPropertyName("tags")]
        public RoleTags? Tags { get; set; }
    }
}
