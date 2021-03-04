// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ValhallaLootList.Server.Discord
{
    public class GuildMember
    {
        [JsonPropertyName("user")]
        public DiscordUser? User { get; set; }

        [JsonPropertyName("nick")]
        public string? Nickname { get; set; }

        [JsonPropertyName("roles")]
        public IEnumerable<long> Roles { get; set; } = Array.Empty<long>();

        [JsonPropertyName("joined_at")]
        public DateTimeOffset JoinedAt { get; set; }

        [JsonPropertyName("premium_since")]
        public DateTimeOffset? PremiumSince { get; set; }

        [JsonPropertyName("deaf")]
        public bool Deaf { get; set; }

        [JsonPropertyName("mute")]
        public bool Mute { get; set; }

        [JsonPropertyName("pending")]
        public bool? Pending { get; set; }

        [JsonPropertyName("permissions")]
        public string? Permissions { get; set; }
    }
}
