// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class MemberAdder
    {
        private readonly InputModel _input = new();
        private List<CharacterDto>? _characters;

        protected override void OnParametersSet()
        {
            if (Team is null) throw new ArgumentNullException(nameof(Team));
        }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _characters = null;
                _characters = await Api.GetAsync<List<CharacterDto>>("api/v1/characters?team=none");
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        private async Task OnSubmit()
        {
            try
            {
                var response = await Api.PostAsync($"api/v1/teams/{Team.Id}/members/{_input.Id}");

                if (response.IsSuccessStatusCode)
                {
                    _characters?.RemoveAll(c => c.Id == _input.Id);
                    _modal?.Hide();
                    await MemberAdded.InvokeAsync();
                }
                else
                {
                    var problemDto = await response.Content.ReadFromJsonAsync<ProblemDetails>(Api.JsonSerializerOptions);

                    if (problemDto?.Errors != null)
                    {
                        _serverValidator?.DisplayErrors(problemDto.Errors);
                    }
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        private class InputModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string? Id { get; set; }
        }
    }
}
