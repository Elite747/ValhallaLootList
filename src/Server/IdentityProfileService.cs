// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server
{
    public class IdentityProfileService : DefaultProfileService
    {
        private readonly DiscordClientProvider _discordClientProvider;
        private readonly ApplicationDbContext _context;

        public IdentityProfileService(DiscordClientProvider discordClientProvider, ILogger<DefaultProfileService> logger, ApplicationDbContext context) : base(logger)
        {
            _discordClientProvider = discordClientProvider;
            _context = context;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);
            //context.IssuedClaims.AddRange(context.Subject.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));

            if (long.TryParse(context.Subject.GetSubjectId(), out var userId))
            {
                // add character and raid leader claims
                var claims = await _context.UserClaims
                    .AsNoTracking()
                    .Where(claim => claim.UserId == userId && (claim.ClaimType == AppClaimTypes.Character || claim.ClaimType == AppClaimTypes.RaidLeader))
                    .Select(claim => new Claim(claim.ClaimType, claim.ClaimValue))
                    .ToListAsync();

                context.IssuedClaims.AddRange(claims);

                var member = await _discordClientProvider.GetMemberAsync(userId);

                if (member is not null)
                {
                    // add discord claims
                    context.IssuedClaims.Add(new Claim(DiscordClaimTypes.AvatarHash, member.AvatarHash));
                    context.IssuedClaims.Add(new Claim(DiscordClaimTypes.AvatarUrl, member.AvatarUrl));
                    context.IssuedClaims.Add(new Claim(DiscordClaimTypes.Discriminator, member.Discriminator));
                    context.IssuedClaims.Add(new Claim(DiscordClaimTypes.Username, member.Username));

                    // add discord role claims
                    foreach (var role in member.Roles)
                    {
                        context.IssuedClaims.Add(new Claim(DiscordClaimTypes.Role, role.Name));
                    }

                    // add app role claims
                    foreach (var appRole in _discordClientProvider.GetAppRoles(member))
                    {
                        context.IssuedClaims.Add(new Claim(AppClaimTypes.Role, appRole));
                    }
                }
            }
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = false;

            if (long.TryParse(context.Subject.GetSubjectId(), out var id))
            {
                var guildMember = await _discordClientProvider.GetMemberAsync(id);

                if (guildMember is not null)
                {
                    context.IsActive = _discordClientProvider.IsInAppRole(guildMember, AppRoles.Member);
                }
            }
        }
    }
}