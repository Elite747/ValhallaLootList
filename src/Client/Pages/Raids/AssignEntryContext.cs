// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Globalization;
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

                foreach (var bonus in entry.Bonuses)
                {
                    Prio += bonus.Value;
                }

                AvatarContent = string.Format(CultureInfo.CurrentCulture, "{0:N0}", Prio);

                if (!entry.Locked)
                {
                    Color = Color.Error;
                    Message = "Loot List is not locked!";
                }
                else if (!entry.Approved)
                {
                    Color = Color.Error;
                    Message = "Loot List has not been approved!";
                }
                else
                {
                    Color = Color.Success;
                    Message = string.Empty;
                }
            }
            else
            {
                AvatarContent = "!";
                Color = Color.Error;

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

        public string AvatarContent { get; }

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
