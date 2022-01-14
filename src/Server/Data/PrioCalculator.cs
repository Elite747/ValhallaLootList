﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Data;

public static class PrioCalculator
{
    public static int CalculateAttendanceBonus(int attendances, PriorityScope scope)
    {
        return (int)Math.Floor((double)Math.Min(attendances, scope.ObservedAttendances) / scope.AttendancesPerPoint);
    }

    public static IEnumerable<PriorityBonusDto> GetListBonuses(
        PriorityScope scope,
        int attendances,
        RaidMemberStatus status,
        long donatedCopper,
        bool enchanted,
        bool prepared)
    {
        yield return GetAttendanceBonus(scope, attendances);
        yield return GetStatusBonus(scope, status);
        yield return GetDonationBonus(scope, donatedCopper);
        yield return GetEnchantedBonus(enchanted);
        yield return GetPreparedBonus(prepared);

        static PriorityBonusDto GetAttendanceBonus(PriorityScope scope, int attendances)
        {
            return new AttendancePriorityBonusDto
            {
                Type = PriorityBonusTypes.Attendance,
                Value = (int)Math.Floor((double)Math.Min(attendances, scope.ObservedAttendances) / scope.AttendancesPerPoint),
                AttendancePerPoint = scope.AttendancesPerPoint,
                Attended = attendances,
                ObservedAttendances = scope.ObservedAttendances
            };
        }

        static PriorityBonusDto GetStatusBonus(PriorityScope scope, RaidMemberStatus status)
        {
            return new MembershipPriorityBonusDto
            {
                Type = PriorityBonusTypes.Trial,
                Value = status switch
                {
                    RaidMemberStatus.HalfTrial => scope.HalfTrialPenalty,
                    RaidMemberStatus.FullTrial => scope.FullTrialPenalty,
                    RaidMemberStatus.Member => 0,
                    _ => throw new ArgumentOutOfRangeException(nameof(status))
                },
                Status = status
            };
        }

        static PriorityBonusDto GetDonationBonus(PriorityScope scope, long donatedCopper)
        {
            return new DonationPriorityBonusDto
            {
                Type = PriorityBonusTypes.Donation,
                Value = donatedCopper >= scope.RequiredDonationCopper ? 1 : 0,
                DonatedCopper = donatedCopper,
                RequiredDonations = scope.RequiredDonationCopper
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

    public static IEnumerable<PriorityBonusDto> GetAllBonuses(
        PriorityScope scope,
        int attendances,
        RaidMemberStatus status,
        long donatedCopper,
        int timesSeen,
        bool enchanted,
        bool prepared)
    {
        return GetListBonuses(scope, attendances, status, donatedCopper, enchanted, prepared).Concat(GetItemBonuses(timesSeen));
    }
}
