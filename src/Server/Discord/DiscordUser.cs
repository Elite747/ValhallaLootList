// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Server.Discord
{
    public class DiscordUser
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; } = string.Empty;

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }

        [JsonPropertyName("bot")]
        public bool? Bot { get; set; }

        [JsonPropertyName("system")]
        public bool? System { get; set; }

        [JsonPropertyName("mfa_enabled")]
        public bool MfaEnabled { get; set; }

        [JsonPropertyName("locale")]
        public string? Locale { get; set; }

        [JsonPropertyName("verified")]
        public bool? Verified { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("flags")]
        public UserFlags? Flags { get; set; }

        [JsonPropertyName("premium_type")]
        public PremiumType? PremiumType { get; set; }

        [JsonPropertyName("public_flags")]
        public UserFlags? PublicFlags { get; set; }
    }
}
