// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server
{
    public class IdentityProfileService : DefaultProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DiscordService _discordService;
        private readonly DiscordRoleMap _roles;

        public IdentityProfileService(UserManager<AppUser> userManager, DiscordService discordService, DiscordRoleMap roles, ILogger<DefaultProfileService> logger) : base(logger)
        {
            _userManager = userManager;
            _discordService = discordService;
            _roles = roles;
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));
            return base.GetProfileDataAsync(context);
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;

            string idString = context.Subject.GetSubjectId();

            if (long.TryParse(idString, out var id))
            {
                var user = await _userManager.FindByIdAsync(idString);

                if (user is not null)
                {
                    var oldClaims = await _userManager.GetClaimsAsync(user);

                    var guildMember = await _discordService.GetMemberInfoAsync(id);

                    if (guildMember is not null)
                    {
                        var oldAppRoles = oldClaims.Where(claim => claim.Type == AppClaimTypes.Role).Select(claim => claim.Value).ToHashSet();
                        var newAppRoles = new HashSet<string>();

                        foreach (var (appRole, discordRole) in _roles.AllRoles)
                        {
                            if (guildMember.RoleNames?.Contains(discordRole) == true)
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
}