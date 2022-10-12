// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public static class HelperQueries
{
    public static async Task<List<MemberDto>> GetMembersAsync(ApplicationDbContext context, TimeZoneInfo timeZone, long teamId, byte teamSize, bool isLeader, long? characterId = null)
    {
        var now = timeZone.TimeZoneNow();

        if (now.Hour < 3)
        {
            // as a correction for when raids run past midnight, treat dates passed in here within 2 hours of midnight as the previous day.
            // This way bonuses don't change at midnight when a raid is still likely to be running.
            now = now.AddHours(-now.Hour - 1);
        }

        var thisMonth = new DateTime(now.Year, now.Month, 1);
        var nextMonth = thisMonth.AddMonths(1);

        var members = new List<MemberDto>();

        var query = context.TeamMembers.Where(m => m.TeamId == teamId);

        if (characterId.HasValue)
        {
            query = query.Where(m => m.CharacterId == characterId.Value);
        }

        await foreach (var character in query
            .Select(c => new
            {
                c.Character!.Class,
                c.CharacterId,
                c.Character.Name,
                c.Character.Race,
                c.Character.IsFemale,
                c.JoinedAt,
                c.Enchanted,
                c.Prepared,
                c.Disenchanter,
                c.Bench,
                DonationsForThisMonth = c.Character.Donations.Count(d => d.TargetMonth == thisMonth.Month && d.TargetYear == thisMonth.Year),
                DonationsForNextMonth = c.Character.Donations.Count(d => d.TargetMonth == nextMonth.Month && d.TargetYear == nextMonth.Year),
                Verified = c.Character.VerifiedById.HasValue,
                Teams = c.Character.Teams.Select(t => t.TeamId).ToList(),
                LootLists = c.Character.CharacterLootLists.Where(ll => ll.Size == teamSize).Select(l => new
                {
                    l.MainSpec,
                    l.ApprovedBy,
                    ApprovedByName = l.ApprovedBy.HasValue ? context.Users.Where(u => u.Id == l.ApprovedBy).Select(u => u.UserName).FirstOrDefault() : null,
                    l.Status,
                    l.Phase
                }).ToList(),
            })
            .AsSingleQuery()
            .AsAsyncEnumerable())
        {
            var memberDto = new MemberDto
            {
                Character = new()
                {
                    Class = character.Class,
                    Gender = character.IsFemale ? Gender.Female : Gender.Male,
                    Id = character.CharacterId,
                    Name = character.Name,
                    Race = character.Race,
                    Verified = character.Verified,
                    Teams = character.Teams
                },
                Donations = new()
                {
                    CharacterId = character.CharacterId,
                    ThisMonth = character.DonationsForThisMonth,
                    NextMonth = character.DonationsForNextMonth,
                    Maximum = character.Bench ? 1 : PrioCalculator.MaxDonations
                },
                Enchanted = character.Enchanted,
                JoinedAt = character.JoinedAt,
                Prepared = character.Prepared,
                Disenchanter = character.Disenchanter,
                Bench = character.Bench
            };

            foreach (var lootList in character.LootLists.OrderBy(ll => ll.Phase))
            {
                var lootListDto = new MemberLootListDto
                {
                    Status = lootList.Status,
                    MainSpec = lootList.MainSpec,
                    Phase = lootList.Phase
                };

                if (isLeader)
                {
                    if (lootList.ApprovedBy.HasValue)
                    {
                        lootListDto.Approved = true;
                        lootListDto.ApprovedBy = lootList.ApprovedByName;
                    }
                    else
                    {
                        lootListDto.Approved = false;
                    }
                }

                memberDto.LootLists.Add(lootListDto);
            }

            members.Add(memberDto);
        }

        var currentPhaseStart = await context.PhaseDetails.OrderByDescending(p => p.StartsAt).Select(p => p.StartsAt).FirstAsync();

        var attendanceRecords = await context.RaidAttendees.AsNoTracking()
            .Where(a => a.RemovalId == null && a.Raid.RaidTeamId == teamId && a.Raid.StartedAt >= currentPhaseStart && (characterId == null || a.CharacterId == characterId))
            .Select(a => new { a.CharacterId, a.Raid.StartedAt })
            .ToListAsync();

        var raidsThisPhase = await context.Raids.AsNoTracking()
            .Where(r => r.RaidTeamId == teamId && r.StartedAt >= currentPhaseStart)
            .Select(r => r.StartedAt)
            .ToListAsync();

        foreach (var member in members)
        {
            int attendancesThisPhase = 0, attendancesTotal = 0;

            foreach (var attendanceRecord in attendanceRecords)
            {
                if (attendanceRecord.CharacterId == member.Character.Id && attendanceRecord.StartedAt >= member.JoinedAt && attendanceRecord.StartedAt.Date < now.Date)
                {
                    attendancesTotal++;
                    if (attendanceRecord.StartedAt >= currentPhaseStart)
                    {
                        attendancesThisPhase++;
                    }
                }
            }

            member.Absences = raidsThisPhase.Count(raidDate => raidDate >= member.JoinedAt && raidDate.Date < now.Date) - attendancesThisPhase;
            member.Status = PrioCalculator.GetStatus(teamSize, attendancesTotal);
        }

        return members;
    }
}
