// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class Create
    {
        private readonly TeamSubmissionDto _team;
        private readonly EditContext _editContext;

        public Create()
        {
            _team = new();
            _editContext = new(_team);
        }

        private void AddScheduleClicked()
        {
            _team.Schedules.Add(new());
        }

        private void RemoveScheduleClicked(int index)
        {
            _team.Schedules.RemoveAt(index);
        }

        private async Task OnSubmit()
        {
            if (_editContext.Validate())
            {
                try
                {
                    var response = await Api.PostAsync("api/v1/teams", _team);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseDto = await response.Content.ReadFromJsonAsync<TeamDto>(Api.JsonSerializerOptions);

                        Nav.NavigateTo("/teams/" + responseDto?.Name);
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
        }
    }
}
