// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Authorization
{
    public class TeamLeaderPolicyHandler : AuthorizationHandler<CharacterOwnerRequirement>
    {
        private readonly PermissionManager _permissionManager;

        public TeamLeaderPolicyHandler(PermissionManager permissionManager)
        {
            _permissionManager = permissionManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CharacterOwnerRequirement requirement)
        {
            if (context.User.IsAdmin() && requirement.AllowAdmin)
            {
                context.Succeed(requirement);
                return;
            }

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
