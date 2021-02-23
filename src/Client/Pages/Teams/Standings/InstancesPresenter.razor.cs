// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace ValhallaLootList.Client.Pages.Teams.Standings
{
    public partial class InstancesPresenter
    {
        protected override async Task OnInitializedAsync()
        {
            if (Instances.RequiresLoading)
            {
                try
                {
                    await Instances.EnsureLoadedAsync();
                    StateHasChanged();
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
            }
        }
    }
}
