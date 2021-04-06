// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    public static class HelperQueries
    {
        public static async IAsyncEnumerable<MemberDto> GetMembersAsync(DiscordService discordService, TimeZoneInfo timeZone, IQueryable<Character> characterQuery, PriorityScope scope, long teamId, string teamName, bool isLeader)
        {
            var now = timeZone.TimeZoneNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var nextMonth = thisMonth.AddMonths(1);

            await foreach (var character in characterQuery
                .Select(c => new
                {
                    c.Class,
                    c.Id,
                    c.Name,
                    c.Race,
                    c.IsFemale,
                    c.MemberStatus,
                    Verified = c.VerifiedById.HasValue,
                    LootLists = c.CharacterLootLists.Select(l => new { l.MainSpec, l.ApprovedBy, l.Status, l.Phase }).ToList(),
                    DonatedThisMonth = c.Donations.Where(d => d.Month == thisMonth.Month && d.Year == thisMonth.Year).Sum(d => (long)d.CopperAmount),
                    DonatedNextMonth = c.Donations.Where(d => d.Month == nextMonth.Month && d.Year == nextMonth.Year).Sum(d => (long)d.CopperAmount),
                    Attendance = c.Attendances.Where(x => !x.IgnoreAttendance && x.Raid.RaidTeamId == teamId)
                        .Select(x => x.Raid.StartedAt.Date)
                        .Distinct()
                        .OrderByDescending(x => x)
                        .Take(scope.ObservedAttendances)
                        .Count()
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
                        Id = character.Id,
                        Name = character.Name,
                        Race = character.Race,
                        TeamId = teamId,
                        TeamName = teamName
                    },
                    Status = character.MemberStatus,
                    Verified = character.Verified,
                    DonatedThisMonth = character.DonatedThisMonth,
                    DonatedNextMonth = character.DonatedNextMonth,
                    ThisMonthRequiredDonations = scope.RequiredDonationCopper,
                    NextMonthRequiredDonations = scope.RequiredDonationCopper,
                    Attendance = character.Attendance,
                    AttendanceMax = scope.ObservedAttendances
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
                            lootListDto.ApprovedBy = await discordService.GetGuildMemberDtoAsync(lootList.ApprovedBy);
                        }
                        else
                        {
                            lootListDto.Approved = false;
                        }
                    }

                    memberDto.LootLists.Add(lootListDto);
                }

                yield return memberDto;
            }
        }
    }
}
