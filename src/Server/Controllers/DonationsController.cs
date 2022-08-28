// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdGen;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public class DonationsController : ApiControllerV1
{
    private readonly ApplicationDbContext _context;
    private readonly TimeZoneInfo _serverTimeZone;
    private readonly IIdGenerator<long> _idGenerator;
    private readonly TelemetryClient _telemetry;

    public DonationsController(
        ApplicationDbContext context,
        TimeZoneInfo serverTimeZone,
        IIdGenerator<long> idGenerator,
        TelemetryClient telemetry)
    {
        _context = context;
        _serverTimeZone = serverTimeZone;
        _idGenerator = idGenerator;
        _telemetry = telemetry;
    }

    [HttpPost, Authorize(AppPolicies.Administrator)]
    public async Task<IActionResult> Post([FromBody] DonationSubmissionDto dto)
    {
        var donatedAt = _serverTimeZone.TimeZoneNow();
        var nextMonth = donatedAt.AddMonths(1);

        var character = await _context.Characters.FindAsync(dto.CharacterId);

        if (character is null)
        {
            return NotFound();
        }

        if ((dto.TargetMonth != donatedAt.Month || dto.TargetYear != donatedAt.Year) &&
            (dto.TargetMonth != nextMonth.Month || dto.TargetYear != nextMonth.Year))
        {
            return Problem("Donations can only target the current or next calendar month.");
        }

        _context.Donations.Add(new Donation(_idGenerator.CreateId())
        {
            Amount = dto.Amount,
            TargetMonth = dto.TargetMonth,
            TargetYear = dto.TargetYear,
            Unit = dto.Unit ?? "Unknown",
            DonatedAt = donatedAt,
            Character = character,
            CharacterId = character.Id,
            EnteredById = User.GetDiscordId().GetValueOrDefault()
        });

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("DonationAdded", User);

        return Ok();
    }
}
