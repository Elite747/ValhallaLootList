// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MudBlazor;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public partial class Create
    {
        private static readonly Dictionary<PlayerRace, Classes[]> _classLookup = new()
        {
            [PlayerRace.Human] = new[] { Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warlock, Classes.Warrior },
            [PlayerRace.Dwarf] = new[] { Classes.Hunter, Classes.Paladin, Classes.Priest, Classes.Rogue, Classes.Warrior },
            [PlayerRace.NightElf] = new[] { Classes.Druid, Classes.Hunter, Classes.Priest, Classes.Rogue, Classes.Warrior },
            [PlayerRace.Gnome] = new[] { Classes.Mage, Classes.Rogue, Classes.Warlock, Classes.Warrior },
            [PlayerRace.Draenei] = new[] { Classes.Hunter, Classes.Mage, Classes.Paladin, Classes.Priest, Classes.Shaman, Classes.Warrior }
        };

        private readonly CharacterSubmissionDto _character = new();
        private Classes[] _raceClasses = Array.Empty<Classes>();

        private Task OnSubmit()
        {
            return (EditingCharacter is null ? Api.Characters.Create(_character) : Api.Characters.Update(EditingCharacter.Id, _character))
                .OnSuccess(character => Dialog.Close(DialogResult.Ok(character)))
                .ValidateWith(_problemValidator)
                .ExecuteAsync();
        }

        private void RaceChanged(PlayerRace? newValue)
        {
            _character.Race = newValue;
            _raceClasses = (newValue.HasValue && _classLookup.TryGetValue(newValue.Value, out var raceClasses)) ? raceClasses : Array.Empty<Classes>();

            if (Array.IndexOf(_raceClasses, _character.Class) < 0)
            {
                _character.Class = 0;
            }
        }
    }
}
