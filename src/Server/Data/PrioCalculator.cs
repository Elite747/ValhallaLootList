﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public PrioCalculator(ApplicationDbContext context)
        {
            _context = context;
        }

        public record PrioCalculationResult(int? Priority, bool IsUsable, string? UnusableReason);

        public async Task<PrioCalculationResult> CalculatePrioAsync(string characterId, uint itemId)
        {
            var entry = await _context.LootListEntries
                .AsNoTracking()
                .Where(e => e.ItemId == itemId && e.LootList.CharacterId == characterId)
                .Select(e => new
                {
                    e.Rank,
                    e.LootList.Locked,
                    e.LootList.Character.MemberStatus,
                    TeamId = (string?)e.LootList.Character.Team!.Id
                })
                .FirstOrDefaultAsync();

            if (entry is null)
            {
                return new(null, false, "Character does not have the item on their list");
            }

            if (string.IsNullOrEmpty(entry.TeamId))
            {
                return new(null, false, "Character is not on a raid team.");
            }

            if (await _context.Drops.AnyAsync(drop => drop.ItemId == itemId && drop.Winner!.Id == characterId))
            {
                return new(null, false, "Character already won the item.");
            }

            var lossRecords = await _context.DropPasses
                .AsNoTracking()
                .Where(dp => dp.Drop.ItemId == itemId && dp.CharacterId == characterId)
                .Select(dp => dp.RelativePriority)
                .ToListAsync();

            var attendances = await _context.Raids
                .AsNoTracking()
                .Where(r => r.RaidTeam.Id == entry.TeamId)
                .OrderByDescending(r => r.StartedAtUtc)
                .Select(r => r.Attendees.Any(a => a.CharacterId == characterId))
                .Take(ObservedRaidsForAttendance)
                .Where(x => x)
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
        private static int CalculatePrio(int playerRank, int attendances, RaidMemberStatus memberStatus, int lostCount, int underPrioCount)//, int notDroppedCount)
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
