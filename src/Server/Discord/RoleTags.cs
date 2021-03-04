// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Server.Discord
{
    public class RoleTags
    {
        [JsonPropertyName("bot_id")]
        public long? BotId { get; set; }

        [JsonPropertyName("integration_id")]
        public long? IntegrationId { get; set; }

        [JsonPropertyName("premium_subscriber")]
        public object? PremiumSubscriber { get; set; }
    }
}
