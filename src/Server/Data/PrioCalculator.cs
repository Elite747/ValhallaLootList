// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.Server.Data
{
    public class PrioCalculator
    {
        public const int
            ObservedRaidsForAttendance = 8,
            AttendancesPerPrioPoint = 4,
            TrialWeek1Penalty = -18,
            TrialWeek2Penalty = -9;

        private readonly ApplicationDbContext _context;
        private readonly TimeZoneInfo _serverTimeZoneInfo;

        public PrioCalculator(ApplicationDbContext context, TimeZoneInfo serverTimeZoneInfo)
        {
            _context = context;
            _serverTimeZoneInfo = serverTimeZoneInfo;
        }

        public record PrioCalculationResult(int? Priority, bool Locked, string? Details);

        public async Task<PrioCalculationResult> CalculatePrioAsync(string characterId, uint itemId)
        {
            var entries = await _context.LootListEntries
                .AsNoTracking()
                .Where(e => (e.ItemId == itemId || e.Item!.RewardFromId == itemId) && e.LootList.CharacterId == characterId)
                .Select(e => new
                {
                    e.Rank,
                    e.LootList.Locked,
                    e.LootList.Character.MemberStatus,
                    TeamId = (string?)e.LootList.Character.Team!.Id
                })
                .OrderByDescending(e => e.Rank)
                .ToListAsync();

            if (entries.Count == 0)
            {
                return new(null, false, "Character does not have the item on their list");
            }

            if (string.IsNullOrEmpty(entries[0].TeamId))
            {
                return new(null, entries[0].Locked, "Character is not on a raid team.");
            }

            var winCount = await _context.Drops.AsNoTracking().CountAsync(drop => drop.ItemId == itemId && drop.WinnerId == characterId);

            if (winCount >= entries.Count)
            {
                return new(null, entries[0].Locked, "Character already won the item.");
            }

            var entry = entries[winCount];

            var lossRecords = await _context.DropPasses
                .AsNoTracking()
                .Where(dp => dp.Drop.ItemId == itemId && dp.CharacterId == characterId)
                .Select(dp => dp.RelativePriority)
                .ToListAsync();

            var offset = TimeSpanHelpers.GetTimeZoneOffsetString(_serverTimeZoneInfo.BaseUtcOffset);
            var attendances = await _context.RaidAttendees
                .AsNoTracking()
                .Where(x => !x.IgnoreAttendance && x.CharacterId == characterId && x.Raid.RaidTeamId == entry.TeamId)
                .Select(x => MySqlTranslations.ConvertTz(x.Raid.StartedAtUtc, "+00:00", offset).Date)
                .Distinct()
                .OrderByDescending(x => x)
                .Take(ObservedRaidsForAttendance)
                .CountAsync();

            var lostCount = lossRecords.Count(x => x >= 0);
            var underPrioCount = lossRecords.Count - lostCount;
            /* var notDroppedCount = 0;await _context.BossKills
                .AsNoTracking()
                .Where(kill => kill.Raid.Schedule.RaidTeam.Id == entry.TeamId &&
                               kill.Raid.Attendees.Any(a => !a.IgnoreAttendance && a.CharacterId == characterId) &&
                               kill.Boss.Items.Any(i => i.Id == itemId))
                .CountAsync();*/

            var priority = CalculatePrio(entry.Rank, attendances, entry.MemberStatus, lostCount, underPrioCount);

            if (!entry.Locked)
            {
                return new(priority, false, "Character's loot list is not locked.");
            }

            return new(priority, true, null);
        }

        /// <summary>
        /// Calculates the priority of an item.
        /// </summary>
        /// <param name="playerRank">The player-assigned rank of the item. (1-18)</param>
        /// <param name="attendances">The total number of attended raids that were tracked.</param>
        /// <param name="memberStatus">The trial status of the player.</param>
        /// <param name="lostCount">The number of times the player lost the item to a /roll or voluntarily passing.</param>
        /// <param name="underPrioCount">The number of times the player was not considered for the item due to others having higher priority.</param>
        /// <param name="notDroppedCount">*UNUSED* The number of times the player witnessed the boss die from where the item drops from, but it did not drop.</param>
        /// <returns>The final calculated priority of an item for a player.</returns>
        public static int CalculatePrio(int playerRank, int attendances, RaidMemberStatus memberStatus, int lostCount, int underPrioCount)//, int notDroppedCount)
        {
            int attendanceBonus = (int)Math.Floor((double)attendances / AttendancesPerPrioPoint);

            int passBonus = (lostCount + underPrioCount);

            const int badLuckProtection = 0; //(int)Math.Floor((double)notDroppedCount / ??);

            int trialPenalty = memberStatus switch
            {
                RaidMemberStatus.Member => 0,
                RaidMemberStatus.HalfTrial => TrialWeek2Penalty,
                RaidMemberStatus.FullTrial => TrialWeek1Penalty,
                _ => throw new ArgumentOutOfRangeException(nameof(memberStatus))
            };

            return playerRank + attendanceBonus + passBonus + badLuckProtection + trialPenalty;
        }
    }
}
