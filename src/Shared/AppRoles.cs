// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList
{
    public static class AppRoles
    {
        public const string Member = nameof(Member);

        public const string Administrator = nameof(Administrator);

        public const string RaidLeader = nameof(RaidLeader);

        public const string LootMaster = nameof(LootMaster);

        public static void ConfigureAuthorization(AuthorizationOptions options)
        {
            var memberPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, Member)
                .Build();

            options.DefaultPolicy = memberPolicy;

            options.AddPolicy(Member, memberPolicy);

            options.AddPolicy(Administrator, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, Administrator)
                .Build());

            options.AddPolicy(RaidLeader, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, RaidLeader)
                .Build());

            options.AddPolicy(LootMaster, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, LootMaster, RaidLeader)
                .Build());
        }
    }
}
