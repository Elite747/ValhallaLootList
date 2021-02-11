// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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

        private readonly CharacterSubmissionDto _character;
        private readonly EditContext _editContext;
        private Classes[] _raceClasses;

        public Create()
        {
            _character = new();
            _editContext = new(_character);
            _raceClasses = Array.Empty<Classes>();
        }

        private async Task OnSubmit()
        {
            if (_editContext.Validate())
            {
                try
                {
                    var response = await Api.PostAsync("api/v1/characters", _character);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseDto = await response.Content.ReadFromJsonAsync<CharacterDto>();

                        Nav.NavigateTo("/characters/" + responseDto?.Name);
                    }
                    else
                    {
                        var problemDto = await response.Content.ReadFromJsonAsync<ProblemDetails>();

                        if (problemDto?.Errors != null)
                        {
                            _customValidator?.DisplayErrors(problemDto.Errors);
                        }
                    }
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
            }
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
