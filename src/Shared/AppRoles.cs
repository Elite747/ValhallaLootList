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

        public const string CharacterOwner = nameof(CharacterOwner);

        public const string CharacterOwnerOrAdmin = nameof(CharacterOwnerOrAdmin);

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
                .RequireClaim(AppClaimTypes.Role, Member)
                .RequireAssertion(context => context.Resource switch
                {
                    long teamId => context.User.IsRaidLeader() && context.User.IsLeaderOf(teamId),
                    null => context.User.IsRaidLeader(),
                    _ => false
                })
                .Build());

            options.AddPolicy(LootMaster, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, Member)
                .RequireAssertion(context => context.Resource switch
                {
                    long teamId => context.User.IsLootMaster() && context.User.IsLeaderOf(teamId),
                    null => context.User.IsLootMaster(),
                    _ => false
                })
                .Build());

            options.AddPolicy(CharacterOwner, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, Member)
                .RequireAssertion(context => context.Resource switch
                {
                    long characterId => context.User.IsOwnerOf(characterId),
                    _ => false
                })
                .Build());

            options.AddPolicy(CharacterOwnerOrAdmin, new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim(AppClaimTypes.Role, Member)
                .RequireAssertion(context => context.Resource switch
                {
                    long characterId => context.User.IsAdmin() || context.User.IsOwnerOf(characterId),
                    null => context.User.IsAdmin(),
                    _ => false
                })
                .Build());
        }
    }
}
