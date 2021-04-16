// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace MudBlazor
{
    public class MudTabsEx : MudTabs
    {
        [Parameter] public object? InitialTabId { get; set; }

        protected override void OnAfterRender(bool firstRender)
        {
            base.OnAfterRender(firstRender);

            if (firstRender && InitialTabId is not null && ActivePanel?.ID != InitialTabId)
            {
                ActivatePanel(InitialTabId);
            }
        }
    }
}
