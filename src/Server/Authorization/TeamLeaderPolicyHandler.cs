// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Authorization
{
    public class TeamLeaderPolicyHandler : DiscordAuthorizationHandler<TeamLeaderRequirement>
    {
        private readonly ApplicationDbContext _context;

        public TeamLeaderPolicyHandler(ApplicationDbContext context, DiscordClientProvider discordClientProvider) : base(discordClientProvider)
        {
            _context = context;
        }

        protected override async ValueTask HandleRequirementAsync(AuthorizationHandlerContext context, TeamLeaderRequirement requirement, DiscordMember member)
        {
            if (requirement.AllowAdmin && DiscordClientProvider.HasAdminRole(member))
            {
                context.Succeed(requirement);
                return;
            }

            if ((requirement.AllowRaidLeader && DiscordClientProvider.HasRaidLeaderRole(member)) ||
                (requirement.AllowLootMaster && DiscordClientProvider.HasLootMasterRole(member)) ||
                (requirement.AllowRecruiter && DiscordClientProvider.HasRecruiterRole(member)))
            {
                long? teamId = context.Resource switch
                {
                    long id => id,
                    TeamDto team => team.Id,
                    _ => null
                };

                if (teamId.HasValue)
                {
                    var discordId = (long)member.Id;
                    var teamIdString = teamId.Value.ToString();

                    if (context.User.HasClaim(AppClaimTypes.RaidLeader, teamIdString) ||
                        await _context.UserClaims.AsNoTracking().CountAsync(claim => claim.UserId == discordId && claim.ClaimType == AppClaimTypes.RaidLeader && claim.ClaimValue == teamIdString) > 0)
                    {
                        context.Succeed(requirement);
                    }
                }
                else
                {
                    context.Succeed(requirement);
                }
            }
        }
    }
}
