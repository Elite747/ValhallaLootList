﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.Server.Discord
{
    public class DiscordServiceOptions
    {
        public int? ApiVersion { get; set; }

        public string? BotToken { get; set; }

        public long GuildId { get; set; }

        public Dictionary<string, string> AppRoleMap { get; } = new();
    }
}