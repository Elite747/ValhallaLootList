// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList.Client.Authorization
{
    public class AdminPolicyHandler : AuthorizationHandler<AdminRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminRequirement requirement)
        {
            if (context.User.HasClaim(AppClaimTypes.Role, AppRoles.Administrator))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
