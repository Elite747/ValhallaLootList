// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using IdGen;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public class DonationsController : ApiControllerV1
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZone;
        private readonly IIdGenerator<long> _idGenerator;
        private readonly TelemetryClient _telemetry;

        public DonationsController(ApplicationDbContext context, TimeZoneInfo serverTimeZone, IIdGenerator<long> idGenerator, TelemetryClient telemetry)
        {
            _context = context;
            _serverTimeZone = serverTimeZone;
            _idGenerator = idGenerator;
            _telemetry = telemetry;
        }

        [HttpPost, Authorize(AppPolicies.RaidLeaderOrAdmin)]
        public async Task<IActionResult> Post([FromBody] DonationSubmissionDto dto)
        {
            var character = await _context.Characters.FindAsync(dto.CharacterId);

            if (character is null)
            {
                return NotFound();
            }

            var now = _serverTimeZone.TimeZoneNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var nextMonth = thisMonth.AddMonths(1);

            _context.Donations.Add(new Donation(_idGenerator.CreateId())
            {
                CopperAmount = dto.CopperAmount,
                DonatedAt = now,
                Character = character,
                CharacterId = character.Id,
                EnteredById = User.GetDiscordId().GetValueOrDefault(),
#pragma warning disable CS0618 // Type or member is obsolete
                Month = (byte)nextMonth.Month,
                Year = (short)nextMonth.Year
#pragma warning restore CS0618 // Type or member is obsolete
            });

            await _context.SaveChangesAsync();

            _telemetry.TrackEvent("DonationAdded", User);

            return Ok();
        }
    }
}
