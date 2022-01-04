// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList.Client.Authorization;

public class MemberPolicyHandler : AuthorizationHandler<MemberRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MemberRequirement requirement)
    {
        if (context.User.HasClaim(AppClaimTypes.Role, AppRoles.Member))
        {
            context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}
