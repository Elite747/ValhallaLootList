// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared;

public abstract class WowIcon : ComponentBase
{
    [Parameter] public IconSize Size { get; set; }
    [Parameter] public int? Width { get; set; }
    [Parameter] public int? Height { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public Dictionary<string, object> UserAttributes { get; set; } = new Dictionary<string, object>();

    protected abstract string GetIconId();

    protected virtual string GetAltText()
    {
        return GetIconId();
    }

    protected virtual bool IconReady()
    {
        return true;
    }

    protected sealed override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(1, "img");

        if (IconReady())
        {
            builder.AddAttribute(2, "src", GenerateSourceUri());
        }

        builder.AddAttribute(3, "alt", GetAltText());
        builder.AddAttribute(4, "class", Class);
        builder.AddAttribute(5, "style", Style);
        builder.AddAttribute(6, "width", Width);
        builder.AddAttribute(7, "height", Height);
        builder.AddMultipleAttributes(8, UserAttributes);
        builder.CloseElement();
    }

    private string GenerateSourceUri()
    {
        return "https://wow.zamimg.com/images/wow/icons/" + GetSizeName() + '/' + GetIconId() + (Size == IconSize.Tiny ? ".gif" : ".jpg");
    }

    private string GetSizeName()
    {
        return Size switch
        {
            IconSize.Tiny => "tiny",
            IconSize.Small => "small",
            IconSize.Medium => "medium",
            IconSize.Large => "large",
            _ => throw new ArgumentOutOfRangeException(nameof(Size))
        };
    }
}
