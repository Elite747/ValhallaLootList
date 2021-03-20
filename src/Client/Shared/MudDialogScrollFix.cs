// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace ValhallaLootList.Client.Shared
{
    public class MudDialogScrollFix : ComponentBase
    {
        [Inject] public IJSRuntime JS { get; set; } = null!;

        [CascadingParameter] public MudDialogInstance Dialog { get; set; } = null!;

        protected override void OnInitialized()
        {
            if (JS is null) throw new ArgumentNullException(nameof(JS));
            if (Dialog is null) throw new ArgumentNullException(nameof(Dialog));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JS.InvokeVoidAsync("valhallaLootList.makeDialogScrollable", "_" + Dialog.Id.ToString("N"));
        }
    }
}