﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList.DataTransfer
{
    public class GuildMemberDto
    {
        private List<string>? _discordRoles, _appRoles;

        public long Id { get; set; }

        public string? Nickname { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Discriminator { get; set; } = string.Empty;

        public string? Avatar { get; set; }

        public List<string> DiscordRoles
        {
            get => _discordRoles ??= new();
            set => _discordRoles = value;
        }

        public List<string> AppRoles
        {
            get => _appRoles ??= new();
            set => _appRoles = value;
        }

        public string GetDisplayName() => Nickname ?? Username ?? throw new Exception("No username or nickname was retrieved from Discord.");
    }
}
