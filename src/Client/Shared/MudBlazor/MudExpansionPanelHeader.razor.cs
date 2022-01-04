// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using MudBlazor.Utilities;

namespace MudBlazor;

public partial class MudExpansionPanelHeader : MudExpansionPanelEx
{
    [CascadingParameter] private MudExpansionPanels Parent2 { get; set; } = null!;

    [Parameter] public RenderFragment? ActionsContent { get; set; }

    public MudExpansionPanelHeader()
    {
        Disabled = true;
        CursorEnabled = true;
    }

    protected new string Classname =>
    new CssBuilder("mud-expand-panel")
        .AddClass("mud-panel-expanded", IsExpanded)
        .AddClass("mud-panel-next-expanded", NextPanelExpanded)
        .AddClass($"mud-elevation-{Parent2?.Elevation.ToString()}")
        .AddClass("mud-expand-panel-border", Parent2?.DisableBorders != true)
        .AddClass(Class)
    .Build();
}
