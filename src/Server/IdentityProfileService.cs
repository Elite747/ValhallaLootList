// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server
{
    public class IdentityProfileService : IProfileService
    {
        private readonly UserManager<AppUser> _userManager;

        public IdentityProfileService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.AddRange(context.Subject.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));
            return Task.CompletedTask;
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var userId = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(userId);
            // TODO: validate user is still in the server and has the member role.
            context.IsActive = user != null;
        }
    }
}