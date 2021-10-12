// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public class AssignEntryContext
    {
        public AssignEntryContext(RaidDto raid, EncounterDropDto drop, AttendanceDto attendance, ItemPrioDto? entry)
        {
            Character = attendance.Character;

            var kill = raid.Kills.Find(k => k.Drops.Contains(drop));
            System.Diagnostics.Debug.Assert(kill is not null);

            if (entry is not null)
            {
                Prio = entry.Rank;
                Rank = entry.Rank;
                Bonuses = entry.Bonuses;

                foreach (var bonus in entry.Bonuses)
                {
                    if (bonus.Value != 0)
                    {
                        Prio += bonus.Value;
                    }
                }

                Message = entry.Status switch
                {
                    LootListStatus.Editing => "Loot List has not been submitted!",
                    LootListStatus.Submitted => "Loot List has not been approved!",
                    LootListStatus.Approved => "Loot List is not locked!",
                    LootListStatus.Locked => string.Empty,
                    _ => "Unknown Loot List status!"
                };
            }
            else
            {
                Bonuses = Array.Empty<PriorityBonusDto>();

                if (attendance.Character.TeamId == raid.TeamId)
                {
                    Message = "Not on loot list!";
                }
                else
                {
                    Message = "Not on this team!";
                }
            }

            if (attendance.Rto || !kill.Characters.Contains(attendance.Character.Id))
            {
                const string notPresentMessage = "Not present for this kill!", rtoMessage = notPresentMessage + " (RTO)";
                Message = attendance.Rto ? rtoMessage : notPresentMessage;
                Disabled = true;
            }
        }

        public int? Prio { get; }

        public bool IsError { get; }

        public bool Disabled { get; set; }

        public CharacterDto Character { get; }

        public string Message { get; }

        public int? Rank { get; set; }

        public IList<PriorityBonusDto> Bonuses { get; }
    }
}
