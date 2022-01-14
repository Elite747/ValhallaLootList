// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using IdGen;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IAuthorizationService _authorizationService;

    public DonationsController(
        ApplicationDbContext context,
        TimeZoneInfo serverTimeZone,
        IIdGenerator<long> idGenerator,
        TelemetryClient telemetry,
        IAuthorizationService authorizationService)
    {
        _context = context;
        _serverTimeZone = serverTimeZone;
        _idGenerator = idGenerator;
        _telemetry = telemetry;
        _authorizationService = authorizationService;
    }

    [HttpPost, Authorize(AppPolicies.LootMasterOrAdmin)]
    public async Task<IActionResult> Post([FromBody] DonationSubmissionDto dto)
    {
        var donatedAt = _serverTimeZone.TimeZoneNow();

        var character = await _context.Characters.FindAsync(dto.CharacterId);

        if (character is null)
        {
            return NotFound();
        }
        if (!character.TeamId.HasValue)
        {
            return Problem("Donations can only be applied to characters on a raid team.");
        }

        var authResult = await _authorizationService.AuthorizeAsync(User, character.TeamId.Value, AppPolicies.LootMasterOrAdmin);

        if (!authResult.Succeeded)
        {
            return Unauthorized();
        }

        if (dto.ApplyThisMonth)
        {
            if (await _context.Raids.CountAsync(raid => raid.RaidTeamId == character.TeamId.Value &&
                                                        raid.StartedAt.Month == donatedAt.Month &&
                                                        raid.StartedAt.Year == donatedAt.Year) > 0)
            {
                return Problem("Donations can only be applied to the current month before any raid occurs during the month.");
            }

            donatedAt = donatedAt.AddDays(-donatedAt.Day);
        }

        _context.Donations.Add(new Donation(_idGenerator.CreateId())
        {
            CopperAmount = dto.CopperAmount,
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
