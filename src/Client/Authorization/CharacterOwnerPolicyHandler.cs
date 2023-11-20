// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Authorization;

public class CharacterOwnerPolicyHandler(PermissionManager permissionManager) : AuthorizationHandler<CharacterOwnerRequirement>
{
    private readonly PermissionManager _permissionManager = permissionManager;

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, CharacterOwnerRequirement requirement)
    {
        if (requirement.AllowAdmin && context.User.HasClaim(AppClaimTypes.Role, AppRoles.Administrator))
        {
            context.Succeed(requirement);
            return;
        }

        long? characterId = context.Resource switch
        {
            long id => id,
            CharacterDto character => character.Id,
            _ => null
        };

        if (characterId.HasValue && await _permissionManager.IsOwnerOfAsync(characterId.Value))
        {
            context.Succeed(requirement);
        }
    }
}
