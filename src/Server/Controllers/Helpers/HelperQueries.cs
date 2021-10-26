// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.DataTransfer;
using ValhallaLootList.Helpers;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    public static class HelperQueries
    {
        public static async Task<List<MemberDto>> GetMembersAsync(ApplicationDbContext context, TimeZoneInfo timeZone, IQueryable<Character> characterQuery, PriorityScope scope, long teamId, string teamName, bool isLeader)
        {
            var now = timeZone.TimeZoneNow();
            var thisMonth = new DateTime(now.Year, now.Month, 1);
            var lastMonth = thisMonth.AddMonths(-1);

            var members = new List<MemberDto>();

            await foreach (var character in characterQuery
                .Select(c => new
                {
                    c.Class,
                    c.Id,
                    c.Name,
                    c.Race,
                    c.IsFemale,
                    c.MemberStatus,
                    c.JoinedTeamAt,
                    c.Enchanted,
                    Verified = c.VerifiedById.HasValue,
                    LootLists = c.CharacterLootLists.Select(l => new
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
                        Id = character.Id,
                        Name = character.Name,
                        Race = character.Race,
                        TeamId = teamId,
                        TeamName = teamName,
                        Verified = character.Verified
                    },
                    Enchanted = character.Enchanted,
                    JoinedAt = character.JoinedTeamAt,
                    Status = character.MemberStatus,
                    ThisMonthRequiredDonations = scope.RequiredDonationCopper,
                    NextMonthRequiredDonations = scope.RequiredDonationCopper
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

            var memberIds = members.ConvertAll(m => m.Character.Id);
            var donationMatrix = await context.GetDonationMatrixAsync(d => memberIds.Contains(d.CharacterId), scope);

            foreach (var member in members)
            {
                member.DonatedThisMonth = donationMatrix.GetCreditForMonth(member.Character.Id, now);
                member.DonatedNextMonth = donationMatrix.GetDonatedDuringMonth(member.Character.Id, now);

                if (member.DonatedThisMonth > scope.RequiredDonationCopper)
                {
                    member.DonatedNextMonth += member.DonatedThisMonth - scope.RequiredDonationCopper;
                }
            }

            return members;
        }
    }
}
