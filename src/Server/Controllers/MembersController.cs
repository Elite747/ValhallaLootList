// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class MembersController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet, Authorize(AppPolicies.Administrator)]
        public async Task<IList<GuildMemberDto>> Get([FromQuery] string[]? role)
        {
            string[] claimTypes = { DiscordClaimTypes.AvatarHash, DiscordClaimTypes.Username, DiscordClaimTypes.Discriminator, AppClaimTypes.Role };

            var query = from user in _context.Users.AsNoTracking()
                        join claim in
                            from claim in _context.UserClaims.AsNoTracking()
                            where claimTypes.Contains(claim.ClaimType)
                            select claim
                        on user.Id equals claim.UserId
                        select new
                        {
                            user.Id,
                            user.UserName,
                            claim.ClaimType,
                            claim.ClaimValue
                        };

            var users = new Dictionary<long, GuildMemberDto>();
            var matchesRoles = new HashSet<long>();

            await foreach (var row in query.AsAsyncEnumerable())
            {
                if (!users.TryGetValue(row.Id, out var dto))
                {
                    users[row.Id] = dto = new() { Id = row.Id, Nickname = row.UserName };
                }

                switch (row.ClaimType)
                {
                    case AppClaimTypes.Role:
                        dto.AppRoles.Add(row.ClaimValue);
                        if (role?.Contains(row.ClaimValue, StringComparer.OrdinalIgnoreCase) == true)
                        {
                            matchesRoles.Add(dto.Id);
                        }
                        break;
                    case DiscordClaimTypes.Role:
                        dto.DiscordRoles.Add(row.ClaimValue);
                        break;
                    case DiscordClaimTypes.AvatarHash:
                        dto.Avatar = row.ClaimValue;
                        break;
                    case DiscordClaimTypes.Discriminator:
                        dto.Discriminator = row.ClaimValue;
                        break;
                    case DiscordClaimTypes.Username:
                        dto.Username = row.ClaimValue;
                        break;
                }
            }

            IEnumerable<GuildMemberDto> results = users.Values;

            if (role?.Length > 0)
            {
                results = results.Where(user => matchesRoles.Contains(user.Id));
            }

            return results.OrderBy(user => user.Nickname ?? user.Username).ToList();
        }
    }
}
