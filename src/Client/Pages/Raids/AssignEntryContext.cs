// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using MudBlazor;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public class AssignEntryContext
    {
        public AssignEntryContext(RaidDto raid, EncounterDropDto drop, CharacterDto character, ItemPrioDto? entry)
        {
            Character = character;

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

                (Color, Message) = entry.Status switch
                {
                    LootListStatus.Editing => (Color.Error, "Loot List has not been submitted!"),
                    LootListStatus.Submitted => (Color.Error, "Loot List has not been approved!"),
                    LootListStatus.Approved => (Color.Error, "Loot List is not locked!"),
                    LootListStatus.Locked => (Color.Success, string.Empty),
                    _ => (Color.Error, "Unknown Loot List status!")
                };
            }
            else
            {
                Color = Color.Error;
                Bonuses = Array.Empty<PriorityBonusDto>();

                if (character.TeamId == raid.TeamId)
                {
                    Message = "Not on loot list!";
                }
                else
                {
                    Message = "Not on this team!";
                }
            }

            if (!kill.Characters.Contains(character.Id))
            {
                Color = Color.Error;
                Message = "Not present for this kill!";
                Disabled = true;
            }
        }

        public int? Prio { get; }

        public bool IsError { get; }

        public bool Disabled { get; set; }

        public CharacterDto Character { get; }

        public Color Color { get; private set; }

        public string Message { get; private set; }

        public string Icon => Color == Color.Success ? Icons.Material.Filled.CheckCircleOutline : Icons.Material.Filled.ErrorOutline;

        public int? Rank { get; set; }

        public IList<PriorityBonusDto> Bonuses { get; }

        public void SetTied()
        {
            if (!Disabled)
            {
                Color = Color.Warning;
                Message = "Tied Priority!";
            }
        }
    }
}
