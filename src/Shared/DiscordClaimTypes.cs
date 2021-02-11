// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList
{
    public static class DiscordClaimTypes
    {
        public const string ClaimPrefix = "urn:discord:";

        public const string AvatarHash = ClaimPrefix + "avatar:hash";

        public const string AvatarUrl = ClaimPrefix + "avatar:url";

        public const string Discriminator = ClaimPrefix + "user:discriminator";

        public const string Username = ClaimPrefix + "user:name";
    }
}
