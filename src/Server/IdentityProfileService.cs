// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server
{
    public class IdentityProfileService : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DiscordService _discordService;
        private readonly DiscordRoleMap _roles;

        public IdentityProfileService(UserManager<AppUser> userManager, DiscordService discordService, DiscordRoleMap roles)
        {
            _userManager = userManager;
            _discordService = discordService;
            _roles = roles;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims.Where(claim => claim.Type == AppRoles.ClaimType || claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));
            return Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;
            var user = await _userManager.FindByIdAsync(context.Subject.GetSubjectId());

            if (long.TryParse(user.NormalizedUserName, out var discordMemberId) && user is not null)
            {
                var oldClaims = await _userManager.GetClaimsAsync(user);

                var memberRoles = await _discordService.GetMemberRolesAsync(discordMemberId);

                if (memberRoles is not null)
                {
                    var oldAppRoles = oldClaims.Where(claim => claim.Type == AppRoles.ClaimType).Select(claim => claim.Value).ToHashSet();
                    var newAppRoles = new HashSet<string>();

                    foreach (var (appRole, discordRole) in _roles.AllRoles)
                    {
                        if (memberRoles.Contains(discordRole))
                        {
                            newAppRoles.Add(appRole);
                        }
                    }

                    context.IsActive = newAppRoles.Contains(AppRoles.Member) && oldAppRoles.SetEquals(newAppRoles);
                }
            }
        }
    }
}