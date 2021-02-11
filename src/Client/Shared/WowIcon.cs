// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared
{
    public abstract class WowIcon : ComponentBase
    {
        [Parameter] public IconSize Size { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        protected abstract string GetIconId();

        protected virtual string GetAltText() => GetIconId();

        protected override sealed void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(1, "img");
            builder.AddAttribute(2, "src", GenerateSourceUri());
            builder.AddAttribute(3, "alt", GetAltText());
            builder.AddMultipleAttributes(4, AdditionalAttributes);
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