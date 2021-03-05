// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;

namespace ValhallaLootList.Server.Discord
{
    public class GuildMemberInfo
    {
        public GuildMemberInfo(GuildMember guildMember, IReadOnlySet<string> roleNames)
        {
            GuildMember = guildMember;
            RoleNames = roleNames;
        }

        public GuildMember GuildMember { get; }

        public IReadOnlySet<string> RoleNames { get; }

        public string DisplayName => GuildMember.Nickname ?? GuildMember.User?.Username ?? throw new Exception("No username or nickname was retrieved from Discord.");
    }
}
