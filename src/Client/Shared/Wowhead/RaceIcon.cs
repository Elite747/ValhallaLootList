// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared;

public class RaceIcon : WowIcon
{
    [Parameter] public PlayerRace Race { get; set; }
    [Parameter] public Gender Gender { get; set; }

    protected override string GetIconId()
    {
        var race = Race switch
        {
            PlayerRace.Human => "human",
            PlayerRace.Dwarf => "dwarf",
            PlayerRace.NightElf => "nightelf",
            PlayerRace.Gnome => "gnome",
            PlayerRace.Draenei => "draenei",
            _ => throw new ArgumentOutOfRangeException(nameof(Race))
        };

        var gender = Gender switch
        {
            Gender.Male => "male",
            Gender.Female => "female",
            _ => throw new ArgumentOutOfRangeException(nameof(Gender))
        };

        return string.Join('_', "race", race, gender);
    }

    protected override string GetAltText()
    {
        return Race.GetDisplayName() + " " + Gender.ToString();
    }
}
