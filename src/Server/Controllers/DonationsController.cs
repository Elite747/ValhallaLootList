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

[Authorize(AppPolicies.Administrator)]
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

    [HttpGet]
    public IAsyncEnumerable<DonationDto> Get(int month, int year)
    {
        return _context.Donations.AsNoTracking()
            .Where(donation => donation.TargetMonth == month && donation.TargetYear == year)
            .OrderByDescending(donation => donation.DonatedAt)
            .Select(donation => new DonationDto
            {
                Id = donation.Id,
                DonatedAt = donation.DonatedAt,
                Amount = donation.Amount,
                Unit = donation.Unit,
                EnteredById = donation.EnteredById,
                CharacterId = donation.CharacterId,
                CharacterName = donation.Character.Name
            })
            .AsAsyncEnumerable();
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] DonationSubmissionDto dto)
    {
        var donatedAt = _serverTimeZone.TimeZoneNow();
        var nextMonth = donatedAt.AddMonths(1);

        if ((dto.TargetMonth != donatedAt.Month || dto.TargetYear != donatedAt.Year) &&
            (dto.TargetMonth != nextMonth.Month || dto.TargetYear != nextMonth.Year))
        {
            return Problem("Donations can only target the current or next calendar month.");
        }

        var character = await _context.Characters.FindAsync(dto.CharacterId);

        if (character is null)
        {
            return NotFound();
        }

        var count = await _context.Donations.AsNoTracking()
            .CountAsync(donation => donation.TargetMonth == dto.TargetMonth && donation.TargetYear == dto.TargetYear && donation.CharacterId == dto.CharacterId);

        if (count >= PrioCalculator.MaxDonations)
        {
            return Problem("Character already has the maximum donations for the target month.");
        }

        var donation = new Donation(_idGenerator.CreateId())
        {
            Amount = dto.Amount,
            TargetMonth = (byte)dto.TargetMonth,
            TargetYear = (short)dto.TargetYear,
            Unit = dto.Unit?.Trim().ToLower() ?? "Unknown",
            DonatedAt = donatedAt,
            Character = character,
            CharacterId = character.Id,
            EnteredById = User.GetDiscordId().GetValueOrDefault()
        };

        _context.Donations.Add(donation);

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("DonationAdded", User);

        return Ok(new DonationDto
        {
            Amount = donation.Amount,
            DonatedAt = donation.DonatedAt,
            CharacterId = donation.CharacterId,
            CharacterName = character.Name,
            EnteredById = donation.EnteredById,
            Id = donation.Id,
            Unit = donation.Unit
        });
    }

    [HttpPost("Import")]
    public async Task<IActionResult> Import([FromBody] DonationImportDto dto, bool skipExcess = false)
    {
        var donatedAt = _serverTimeZone.TimeZoneNow();
        var nextMonth = donatedAt.AddMonths(1);

        if ((dto.TargetMonth != donatedAt.Month || dto.TargetYear != donatedAt.Year) &&
            (dto.TargetMonth != nextMonth.Month || dto.TargetYear != nextMonth.Year))
        {
            return Problem("Donations can only target the current or next calendar month.");
        }

        if (dto.Records.Count == 0)
        {
            return Problem("No donations were specified.");
        }

        var counts = await _context.Donations.AsNoTracking()
            .Where(donation => donation.TargetMonth == dto.TargetMonth && donation.TargetYear == dto.TargetYear)
            .GroupBy(donation => donation.CharacterId)
            .Select(g => new { CharacterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(g => g.CharacterId, g => g.Count);

        var reverseCharacterLookup = await _context.Characters.AsNoTracking()
            .Where(c => !c.Deactivated)
            .Select(c => new { c.Id, c.Name })
            .ToDictionaryAsync(c => c.Name, StringComparer.OrdinalIgnoreCase);

        var response = new List<DonationDto>();

        var enteredById = User.GetDiscordId().GetValueOrDefault();

        foreach (var record in dto.Records)
        {
            if (!reverseCharacterLookup.TryGetValue(record.CharacterName, out var character))
            {
                return Problem($"Couldn't find character with the name {record.CharacterName}");
            }

            counts.TryGetValue(character.Id, out var count);

            if (count >= PrioCalculator.MaxDonations)
            {
                if (!skipExcess)
                {
                    return Problem($"{character.Name} has already donated {count} times for the target month.");
                }
            }
            else
            {
                var donation = new Donation(_idGenerator.CreateId())
                {
                    Amount = record.Amount,
                    TargetMonth = (byte)dto.TargetMonth,
                    TargetYear = (short)dto.TargetYear,
                    Unit = record.Unit?.Trim().ToLower() ?? "Unknown",
                    DonatedAt = donatedAt,
                    CharacterId = character.Id,
                    EnteredById = enteredById
                };

                response.Add(new DonationDto
                {
                    Amount = donation.Amount,
                    DonatedAt = donatedAt,
                    CharacterId = character.Id,
                    CharacterName = character.Name,
                    EnteredById = enteredById,
                    Id = donation.Id,
                    Unit = donation.Unit
                });

                _context.Donations.Add(donation);
                counts[character.Id] = counts.GetValueOrDefault(character.Id) + 1;
            }
        }

        await _context.SaveChangesAsync();

        _telemetry.TrackEvent("DonationsImported", User);

        return Ok(response);
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var donation = await _context.Donations.FindAsync(id);

        if (donation is null)
        {
            return NotFound();
        }

        _context.Donations.Remove(donation);

        await _context.SaveChangesAsync();

        var characterName = await _context.Characters
            .AsNoTracking()
            .Where(c => c.Id == donation.CharacterId)
            .Select(c => c.Name)
            .FirstAsync();

        _telemetry.TrackEvent("DonationDeleted", User, props =>
        {
            props["CharacterId"] = donation.CharacterId.ToString();
            props["CharacterName"] = characterName;
        });

        return Ok();
    }
}
