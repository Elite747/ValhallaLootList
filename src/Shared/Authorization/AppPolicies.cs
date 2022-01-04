// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList;

public static class AppPolicies
{
    public const string Member = nameof(Member);

    public const string Administrator = nameof(Administrator);

    public const string RaidLeader = nameof(RaidLeader);

    public const string LootMaster = nameof(LootMaster);

    public const string Recruiter = nameof(Recruiter);

    public const string Leadership = nameof(Leadership);

    public const string CharacterOwner = nameof(CharacterOwner);

    public const string CharacterOwnerOrAdmin = nameof(CharacterOwnerOrAdmin);

    public const string RaidLeaderOrAdmin = nameof(RaidLeaderOrAdmin);

    public const string LootMasterOrAdmin = nameof(LootMasterOrAdmin);

    public const string RecruiterOrAdmin = nameof(RecruiterOrAdmin);

    public const string LeadershipOrAdmin = nameof(LeadershipOrAdmin);

    public static void ConfigureAuthorization(AuthorizationOptions options)
    {
        options.AddPolicy(Member, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new MemberRequirement())
            .Build());

        options.AddPolicy(Administrator, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new AdminRequirement())
            .Build());

        options.AddPolicy(RaidLeader, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: false, allowRaidLeader: true, allowLootMaster: false, allowRecruiter: false))
            .Build());

        options.AddPolicy(RaidLeaderOrAdmin, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: true, allowRaidLeader: true, allowLootMaster: false, allowRecruiter: false))
            .Build());

        options.AddPolicy(LootMaster, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: false, allowRaidLeader: false, allowLootMaster: true, allowRecruiter: false))
            .Build());

        options.AddPolicy(LootMasterOrAdmin, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: true, allowRaidLeader: false, allowLootMaster: true, allowRecruiter: false))
            .Build());

        options.AddPolicy(Recruiter, new AuthorizationPolicyBuilder()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: false, allowRaidLeader: false, allowLootMaster: false, allowRecruiter: true))
            .Build());

        options.AddPolicy(RecruiterOrAdmin, new AuthorizationPolicyBuilder()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: true, allowRaidLeader: false, allowLootMaster: false, allowRecruiter: true))
            .Build());

        options.AddPolicy(Leadership, new AuthorizationPolicyBuilder()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: false, allowRaidLeader: true, allowLootMaster: true, allowRecruiter: true))
            .Build());

        options.AddPolicy(LeadershipOrAdmin, new AuthorizationPolicyBuilder()
            .AddRequirements(new TeamLeaderRequirement(allowAdmin: true, allowRaidLeader: true, allowLootMaster: true, allowRecruiter: true))
            .Build());

        options.AddPolicy(CharacterOwner, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new CharacterOwnerRequirement(allowAdmin: false))
            .Build());

        options.AddPolicy(CharacterOwnerOrAdmin, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new CharacterOwnerRequirement(allowAdmin: true))
            .Build());
    }
}
