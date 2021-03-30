// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class PermissionsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public PermissionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PermissionsDto> Get()
        {
            var dto = new PermissionsDto();

            var userId = User.GetDiscordId();

            await foreach (var claim in _context.UserClaims
                .AsNoTracking()
                .Where(claim => claim.UserId == userId)
                .Where(claim => claim.ClaimType == AppClaimTypes.Character || claim.ClaimType == AppClaimTypes.RaidLeader)
                .AsAsyncEnumerable())
            {
                if (long.TryParse(claim.ClaimValue, out var resourceId))
                {
                    if (claim.ClaimType == AppClaimTypes.Character)
                    {
                        dto.Characters.Add(resourceId);
                    }
                    else if (claim.ClaimType == AppClaimTypes.RaidLeader)
                    {
                        dto.Teams.Add(resourceId);
                    }
                }
            }

            return dto;
        }
    }
}
