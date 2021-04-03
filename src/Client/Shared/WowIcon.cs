// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using MudBlazor;

namespace ValhallaLootList.Client.Shared
{
    public abstract class WowIcon : MudComponentBase
    {
        [Parameter] public IconSize Size { get; set; }
        [Parameter] public int? Width { get; set; }
        [Parameter] public int? Height { get; set; }

        protected abstract string GetIconId();

        protected virtual string GetAltText() => GetIconId();

        protected virtual bool IconReady() => true;

        protected override sealed void BuildRenderTree(RenderTreeBuilder builder)
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

        private string GetSizeName() => Size switch
        {
            IconSize.Tiny => "tiny",
            IconSize.Small => "small",
            IconSize.Medium => "medium",
            IconSize.Large => "large",
            _ => throw new ArgumentOutOfRangeException(nameof(Size))
        };
    }
}