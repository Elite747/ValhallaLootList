// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class RaidStarter
    {
        private readonly RaidSubmissionDto _model = new() { Phase = Constants.CurrentPhase };

        protected override void OnParametersSet()
        {
            if (Team is null) throw new ArgumentNullException(nameof(Team));

            _model.TeamId = Team.Id;
            foreach (var character in Team.Roster)
            {
                Debug.Assert(character.Id?.Length > 0);
                _model.Attendees.Add(character.Id);
            }
        }

        private void ToggleAttendee(string id)
        {
            if (_model.Attendees.Contains(id))
            {
                _model.Attendees.Remove(id);
            }
            else
            {
                _model.Attendees.Add(id);
            }
        }

        private async Task SubmitAsync()
        {
            try
            {
                var response = await Api.PostAsync("api/v1/raids", _model);

                if (response.IsSuccessStatusCode)
                {
                    _modal?.Hide();
                    var responseDto = await response.Content.ReadFromJsonAsync<RaidDto>(Api.JsonSerializerOptions);

                    if (responseDto is not null)
                    {
                        await RaidStarted.InvokeAsync(responseDto);
                    }
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
