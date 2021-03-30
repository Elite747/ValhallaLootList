// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList
{
    public static class AppPolicies
    {
        public const string Member = nameof(Member);

        public const string Administrator = nameof(Administrator);

        public const string RaidLeader = nameof(RaidLeader);

        public const string LootMaster = nameof(LootMaster);

        public const string CharacterOwner = nameof(CharacterOwner);

        public const string CharacterOwnerOrAdmin = nameof(CharacterOwnerOrAdmin);

        public const string RaidLeaderOrAdmin = nameof(RaidLeaderOrAdmin);

        public const string LootMasterOrAdmin = nameof(LootMasterOrAdmin);

        public static void ConfigureAuthorization(AuthorizationOptions options)
        {
            options.AddPolicy(Member, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.Member)
                .Build());

            options.AddPolicy(Administrator, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.Administrator)
                .Build());

            options.AddPolicy(RaidLeader, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.RaidLeader)
                .AddRequirements(new TeamLeaderRequirement(false))
                .Build());

            options.AddPolicy(RaidLeaderOrAdmin, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.RaidLeader, AppRoles.Administrator)
                .AddRequirements(new TeamLeaderRequirement(true))
                .Build());

            options.AddPolicy(LootMaster, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.LootMaster)
                .AddRequirements(new TeamLeaderRequirement(false))
                .Build());

            options.AddPolicy(LootMasterOrAdmin, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.LootMaster, AppRoles.Administrator)
                .AddRequirements(new TeamLeaderRequirement(true))
                .Build());

            options.AddPolicy(CharacterOwner, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.Member)
                .AddRequirements(new CharacterOwnerRequirement(false))
                .Build());

            options.AddPolicy(CharacterOwnerOrAdmin, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, AppRoles.Member, AppRoles.Administrator)
                .AddRequirements(new CharacterOwnerRequirement(true))
                .Build());
        }
    }
}
