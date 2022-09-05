// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Data;

public static class PrioCalculator
{
    private const int _halfTrialPenalty = -9, _fullTrialPenalty = -18;
    public const int MaxDonations = 3;

    public static IEnumerable<PriorityBonusDto> GetListBonuses(
        int absences,
        RaidMemberStatus status,
        int donationTickets,
        bool enchanted,
        bool prepared,
        byte teamSize)
    {
        yield return GetAbsencePenalty(absences);
        yield return GetTrialPenalty(status);

        if (teamSize != 10)
        {
            yield return GetDonationBonus(donationTickets);
            yield return GetEnchantedBonus(enchanted);
            yield return GetPreparedBonus(prepared);
        }

        static PriorityBonusDto GetAbsencePenalty(int absences)
        {
            return new AbsencePriorityBonusDto
            {
                Absences = absences,
                Type = PriorityBonusTypes.Absence,
                Value = Fib(absences - 1)
            };
        }

        static PriorityBonusDto GetTrialPenalty(RaidMemberStatus status)
        {
            return new MembershipPriorityBonusDto
            {
                Type = PriorityBonusTypes.Trial,
                Value = status switch
                {
                    RaidMemberStatus.HalfTrial => _halfTrialPenalty,
                    RaidMemberStatus.FullTrial => _fullTrialPenalty,
                    RaidMemberStatus.Member => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(status))
                },
                Status = status
            };
        }

        static PriorityBonusDto GetDonationBonus(int donationTickets)
        {
            return new DonationPriorityBonusDto
            {
                DonationTickets = donationTickets,
                Type = PriorityBonusTypes.Donation,
                Value = donationTickets,
            };
        }

        static PriorityBonusDto GetEnchantedBonus(bool enchanted)
        {
            return new PriorityBonusDto
            {
                Type = PriorityBonusTypes.Enchanted,
                Value = enchanted ? 2 : 0
            };
        }

        static PriorityBonusDto GetPreparedBonus(bool prepared)
        {
            return new PriorityBonusDto
            {
                Type = PriorityBonusTypes.Prepared,
                Value = prepared ? 1 : 0
            };
        }
    }

    public static IEnumerable<PriorityBonusDto> GetItemBonuses(int timesSeen)
    {
        yield return new LossPriorityBonusDto
        {
            Type = PriorityBonusTypes.Lost,
            Value = timesSeen,
            TimesSeen = timesSeen
        };
    }

    private static int Fib(int i)
    {
        if (i <= 0)
        {
            return 0;
        }

        int left = 0, right = 1;

        for (int i2 = 2; i2 <= i; i2++)
        {
            (left, right) = (right, left + right);
        }

        return right;
    }
}
