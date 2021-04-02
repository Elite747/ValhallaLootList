// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server
{
    public class IdentityProfileService : DefaultProfileService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly DiscordService _discordService;
        private readonly ApplicationDbContext _context;

        public IdentityProfileService(UserManager<AppUser> userManager, DiscordService discordService, ILogger<DefaultProfileService> logger, ApplicationDbContext context) : base(logger)
        {
            _userManager = userManager;
            _discordService = discordService;
            _context = context;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);
            context.IssuedClaims.AddRange(context.Subject.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));

            if (long.TryParse(context.Subject.GetSubjectId(), out var userId))
            {
                var claims = await _context.UserClaims
                    .AsNoTracking()
                    .Where(claim => claim.UserId == userId && (claim.ClaimType == AppClaimTypes.Character || claim.ClaimType == AppClaimTypes.RaidLeader))
                    .Select(claim => new Claim(claim.ClaimType, claim.ClaimValue))
                    .ToListAsync();

                context.IssuedClaims.AddRange(claims);
            }
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

                    var guildMember = await _discordService.GetGuildMemberDtoAsync(id);

                    if (guildMember is not null)
                    {
                        var oldAppRoles = oldClaims.Where(claim => claim.Type == AppClaimTypes.Role).Select(claim => claim.Value).ToHashSet();
                        context.IsActive = guildMember.AppRoles.Contains(AppRoles.Member) && oldAppRoles.SetEquals(guildMember.AppRoles);
                    }
                }
            }
        }
    }
}