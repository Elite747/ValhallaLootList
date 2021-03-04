// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class ConfigController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;

        public ConfigController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("phases")]
        public async Task<ActionResult<PhaseConfigDto>> GetPhaseConfig()
        {
            var dto = new PhaseConfigDto
            {
                CurrentPhase = await _context.GetCurrentPhaseAsync()
            };

            await foreach (var bracket in _context.Brackets.AsNoTracking().OrderBy(b => b.Phase).ThenByDescending(b => b.MaxRank).AsAsyncEnumerable())
            {
                if (!dto.Brackets.TryGetValue(bracket.Phase, out var brackets))
                {
                    dto.Brackets[bracket.Phase] = brackets = new();
                }

                brackets.Add(new BracketDto
                {
                    AllowOffSpec = bracket.AllowOffspec,
                    AllowTypeDuplicates = bracket.AllowTypeDuplicates,
                    MaxItems = bracket.MaxItems,
                    MaxRank = bracket.MaxRank,
                    MinRank = bracket.MinRank
                });
            }

            return dto;
        }
    }
}
