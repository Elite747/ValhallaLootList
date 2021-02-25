// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared
{
    public class ItemLinkText : ComponentBase
    {
        [CascadingParameter] public ItemLinkContext Context { get; set; } = null!;
        [Parameter] public string? Class { get; set; }
        [Parameter] public bool? Bracketize { get; set; }
        [Parameter] public bool? Colorize { get; set; }
        [Parameter] public string? OverrideText { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Context is null)
            {
                throw new InvalidOperationException("ItemLinkText must be within an ItemLink.");
            }

            builder.OpenElement(0, "span");

            string? cssClass;

            if ((Colorize ?? Context.Colorize) && Context.Item is not null)
            {
                if (Class?.Length > 0)
                {
                    cssClass = $"q{Context.Item.Quality} {Class}";
                }
                else
                {
                    cssClass = $"q{Context.Item.Quality}";
                }
            }
            else
            {
                cssClass = Class;
            }

            if (cssClass?.Length > 0)
            {
                builder.AddAttribute(1, "class", cssClass);
            }

            string content = OverrideText ?? Context.OverrideText ?? Context.Item?.Name ?? (Context.Failed ? $"https://www.wowhead.com/item={Context.Id}" : "Loading...");

            if (Bracketize ?? Context.Bracketize)
            {
                content = string.Format("[{0}]", content);
            }

            builder.AddContent(2, content);
            builder.CloseElement();
        }
    }
}
