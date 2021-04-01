// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Data
{
    public static class PrioCalculator
    {
        public static PriorityScope Scope { get; } = new()
        {
            AttendancesPerPoint = 4,
            FullTrialPenalty = -18,
            HalfTrialPenalty = -9,
            ObservedAttendances = 8,
            RequiredDonationCopper = 50_00_00
        }; // TODO: Add this to DB.

        public static int CalculateAttendanceBonus(int attendances, PriorityScope scope)
        {
            return (int)Math.Floor((double)Math.Min(attendances, scope.ObservedAttendances) / scope.AttendancesPerPoint);
        }

        public static IEnumerable<PriorityBonusDto> GetListBonuses(PriorityScope scope, int attendances, RaidMemberStatus status, long donatedCopper)
        {
            yield return GetAttendanceBonus(scope, attendances);

            if (status != RaidMemberStatus.Member)
            {
                yield return GetStatusBonus(scope, status);
            }

            yield return GetDonationBonus(scope, donatedCopper);

            static PriorityBonusDto GetAttendanceBonus(PriorityScope scope, int attendances)
            {
                return new PriorityBonusDto
                {
                    Value = (int)Math.Floor((double)Math.Min(attendances, scope.ObservedAttendances) / scope.AttendancesPerPoint),
                    Description = $"Attended {attendances} of {scope.ObservedAttendances} raids"
                };
            }

            static PriorityBonusDto GetStatusBonus(PriorityScope scope, RaidMemberStatus status)
            {
                return new PriorityBonusDto
                {
                    Value = status switch
                    {
                        RaidMemberStatus.HalfTrial => scope.HalfTrialPenalty,
                        RaidMemberStatus.FullTrial => scope.FullTrialPenalty,
                        _ => throw new ArgumentOutOfRangeException(nameof(status))
                    },
                    Description = "Trial member penalty"
                };
            }

            static PriorityBonusDto GetDonationBonus(PriorityScope scope, long donatedCopper)
            {
                if (donatedCopper >= scope.RequiredDonationCopper)
                {
                    return new PriorityBonusDto
                    {
                        Value = 1,
                        Description = $"Donated at least {MakeGameCurrencyString(scope.RequiredDonationCopper)} last month"
                    };
                }
                else
                {
                    return new PriorityBonusDto
                    {
                        Value = 0,
                        Description = $"Donated {MakeGameCurrencyString(donatedCopper)} of the required {MakeGameCurrencyString(scope.RequiredDonationCopper)} last month"
                    };
                }
            }
        }

        public static IEnumerable<PriorityBonusDto> GetItemBonuses(int timesSeen)
        {
            yield return new PriorityBonusDto
            {
                Value = timesSeen,
                Description = $"Seen this item drop {timesSeen} times without winning"
            };
        }

        public static IEnumerable<PriorityBonusDto> GetAllBonuses(PriorityScope scope, int attendances, RaidMemberStatus status, long donatedCopper, int timesSeen)
        {
            return GetListBonuses(scope, attendances, status, donatedCopper).Concat(GetItemBonuses(timesSeen));
        }

        private static string MakeGameCurrencyString(long amount)
        {
            var goldSilver = Math.DivRem(amount, 100L, out var copper);
            var gold = Math.DivRem(goldSilver, 100L, out var silver);

            var parts = new List<string>(3);

            if (gold != 0)
            {
                parts.Add($"{gold:N0} gold");
            }
            if (silver != 0)
            {
                parts.Add($"{silver:N0} silver");
            }
            if (copper != 0 || amount == 0)
            {
                parts.Add($"{copper:N0} copper");
            }

            return string.Join(", ", parts);
        }
    }
}
