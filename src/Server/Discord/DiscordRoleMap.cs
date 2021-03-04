// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace ValhallaLootList.Server.Discord
{
    public class DiscordRoleMap
    {
        private readonly Dictionary<string, string> _appRoleMap;

        public DiscordRoleMap(IOptions<DiscordServiceOptions> options)
        {
            _appRoleMap = options.Value.AppRoleMap;
            EnsureRoleExists(AppRoles.Administrator);
            EnsureRoleExists(AppRoles.LootMaster);
            EnsureRoleExists(AppRoles.Member);
            EnsureRoleExists(AppRoles.RaidLeader);
        }

        public string Administrator => _appRoleMap[AppRoles.Administrator];

        public string LootMaster => _appRoleMap[AppRoles.LootMaster];

        public string Member => _appRoleMap[AppRoles.Member];

        public string RaidLeader => _appRoleMap[AppRoles.RaidLeader];

        public IReadOnlyDictionary<string, string> AllRoles => _appRoleMap;

        private void EnsureRoleExists(string role)
        {
            if (!_appRoleMap.TryGetValue(role, out var memberRole) || string.IsNullOrEmpty(memberRole))
            {
                throw new Exception($"Discord service options does not have a map for the {role} role.");
            }
        }
    }
}
