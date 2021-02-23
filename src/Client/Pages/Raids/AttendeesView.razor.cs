// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class AttendeesView
    {
        private List<CharacterDto>? _allCharacters;

        protected override void OnParametersSet()
        {
            if (Raid is null) throw new ArgumentNullException(nameof(Raid));
        }

        private async Task OnToggledAsync(bool open)
        {
            if (open && _allCharacters is null)
            {
                _allCharacters = await Api.GetAsync<List<CharacterDto>>($"api/v1/characters?team={Raid.TeamId}");
            }
        }

        private async Task OnAddClickedAsync(CharacterDto character)
        {
            try
            {
                var response = await Api.PostAsync($"api/v1/raids/{Raid.Id}/Attendees", new AttendeeSubmissionDto { CharacterId = character.Id });

                if (response.IsSuccessStatusCode)
                {
                    Raid.Attendees.Add(character);
                    StateHasChanged();
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        private async Task OnRemoveClickedAsync(CharacterDto character)
        {
            try
            {
                var response = await Api.DeleteAsync($"api/v1/raids/{Raid.Id}/Attendees/{character.Id}");

                if (response.IsSuccessStatusCode)
                {
                    Raid.Attendees.RemoveAll(c => c.Id == character.Id);
                    StateHasChanged();
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }
    }
}
