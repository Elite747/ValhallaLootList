// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class View
    {
        private TeamCharacterDto? _removingCharacter;

        private void RequestRemoveMember(TeamCharacterDto character)
        {
            _removingCharacter = character;
            _removeModal?.Show();
        }

        private Task RemoveMemberConfirmedAsync(TeamDto team)
        {
            Debug.Assert(_removingCharacter?.Id?.Length > 0);
            _removeModal?.Hide();

            return Api.Teams.RemoveMember(team.Id, _removingCharacter.Id)
                .OnSuccess(_ =>
                {
                    team.Roster.Remove(_removingCharacter);
                    StateHasChanged();
                })
                .SendErrorTo(ErrorHandler)
                .ExecuteAsync();
        }

        private void OnRaidStarted(RaidDto response)
        {
            Nav.NavigateTo("/raids/" + response.Id);
        }
    }
}
