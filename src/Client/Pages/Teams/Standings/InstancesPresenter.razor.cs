// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams.Standings
{
    public partial class InstancesPresenter
    {
        private List<InstanceDto>? _instances;

        protected override async Task OnInitializedAsync()
        {
            _instances = null;
            try
            {
                _instances = await Api.GetAsync<List<InstanceDto>>("api/v1/instances");
                StateHasChanged();
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }
    }
}
