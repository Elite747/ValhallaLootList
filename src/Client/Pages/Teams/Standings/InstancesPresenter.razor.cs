// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams.Standings
{
    public partial class InstancesPresenter
    {
        private readonly List<(byte Phase, IEnumerable<InstanceDto> Instances)> _instances = new();

        protected override async Task OnInitializedAsync()
        {
            if (Instances.RequiresLoading)
            {
                try
                {
                    await Instances.EnsureLoadedAsync();
                    _instances.Clear();

                    foreach (var phase in await PhaseConfig.GetPhasesAsync())
                    {
                        _instances.Add((phase, Instances.GetCached().Where(i => i.Phase == phase).OrderBy(i => i.Name)));
                    }

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
