// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Authorization;

public class TeamLeaderPolicyHandler(PermissionManager permissionManager) : AuthorizationHandler<TeamLeaderRequirement>
{
    private readonly PermissionManager _permissionManager = permissionManager;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TeamLeaderRequirement requirement)
    {
        if (requirement.AllowAdmin && context.User.HasClaim(AppClaimTypes.Role, AppRoles.Administrator))
        {
            context.Succeed(requirement);
            return;
        }

        if ((requirement.AllowLootMaster && context.User.HasClaim(AppClaimTypes.Role, AppRoles.LootMaster)) ||
            (requirement.AllowRaidLeader && context.User.HasClaim(AppClaimTypes.Role, AppRoles.RaidLeader)) ||
            (requirement.AllowRecruiter && context.User.HasClaim(AppClaimTypes.Role, AppRoles.Recruiter)))
        {
            long? teamId = context.Resource switch
            {
                long id => id,
                TeamDto team => team.Id,
                _ => null
            };

            if (teamId is null || await _permissionManager.IsLeaderOfAsync(teamId.Value))
            {
                context.Succeed(requirement);
            }
        }
    }
}
