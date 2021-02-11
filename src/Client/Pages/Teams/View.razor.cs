// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class View
    {
        private TeamDto? _team;
        private bool _notFound;
        private TeamCharacterDto? _removingCharacter;

        protected override Task OnParametersSetAsync()
        {
            return RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            _team = null;
            if (!string.IsNullOrWhiteSpace(Team))
            {
                try
                {
                    _team = await Api.GetAsync<TeamDto>("api/v1/teams/" + Team);
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
                catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    _notFound = true;
                }
            }
        }

        private void RequestRemoveMember(TeamCharacterDto character)
        {
            _removingCharacter = character;
            _removeModal?.Show();
        }

        private async Task RemoveMemberConfirmedAsync()
        {
            Debug.Assert(_team is not null);
            Debug.Assert(_removingCharacter is not null);
            try
            {
                var response = await Api.DeleteAsync($"api/v1/teams/{_team.Id}/members/{_removingCharacter.Id}");

                if (response.IsSuccessStatusCode)
                {
                    _team.Roster.Remove(_removingCharacter);
                    _removeModal?.Hide();
                    StateHasChanged();
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
        }
    }
}
