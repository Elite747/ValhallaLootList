// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class DonationMatrix
{
    private readonly Dictionary<(long characterId, int year, int month), (long donated, long rollover)> _matrix = new();

    public DonationMatrix(List<MonthDonations> donations, PriorityScope scope)
    {
        foreach (var donationsByCharacter in donations.GroupBy(d => d.CharacterId))
        {
            var donationsLookup = donationsByCharacter.ToDictionary(d => new DateTime(d.Year, d.Month, 1), d => d.Donated);

            var oldestDate = donationsLookup.Keys.OrderBy(d => d).First();
            var now = DateTime.UtcNow;
            long rollover = 0;

            for (DateTime date = oldestDate; date < now; date = date.AddMonths(1))
            {
                donationsLookup.TryGetValue(date, out var donated);
                _matrix.Add((donationsByCharacter.Key, date.Year, date.Month), (donated, rollover));
                rollover = Math.Max(0, rollover + donated - scope.RequiredDonationCopper);
            }
        }
    }

    private (long donated, long rollover) GetRecord(long characterId, int month, int year)
    {
        if (_matrix.TryGetValue((characterId, year, month), out var record))
        {
            return record;
        }
        return (0, 0);
    }

    public long GetCreditForMonth(long characterId, DateTimeOffset month)
    {
        month = month.AddMonths(-1); // donations made during a month do not count for that month.
        var (donated, rollover) = GetRecord(characterId, month.Month, month.Year);
        return donated + rollover;
    }

    public long GetDonatedDuringMonth(long characterId, DateTimeOffset month)
    {
        var (donated, _) = GetRecord(characterId, month.Month, month.Year);
        return donated;
    }
}
