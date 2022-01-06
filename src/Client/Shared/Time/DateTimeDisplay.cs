// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared;

public sealed class DateTimeDisplay : ComponentBase, IDisposable
{
    [Parameter] public DateTimeOffset Date { get; set; }
    [Parameter] public string? Format { get; set; }
    [Parameter] public CultureInfo? Culture { get; set; }
    [Inject] public UserTimeProvider TimeProvider { get; set; } = null!;

    protected override void OnInitialized()
    {
        if (TimeProvider is null) throw new ArgumentNullException(nameof(TimeProvider));

        TimeProvider.ModeChanged += StateHasChanged;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddContent(1, TimeProvider.Convert(Date).ToString(Format ?? "f", Culture ?? CultureInfo.CurrentCulture));
        builder.CloseElement();
    }

    void IDisposable.Dispose()
    {
        TimeProvider.ModeChanged -= StateHasChanged;
    }
}
