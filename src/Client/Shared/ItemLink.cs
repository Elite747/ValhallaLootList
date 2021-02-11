// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared
{
    public class ItemLink : ComponentBase
    {
        [Parameter] public uint? Id { get; set; }
        [Parameter] public bool LinkEnabled { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(1, "a");
            builder.AddMultipleAttributes(2, AdditionalAttributes);

            if (Id > 0)
            {
                builder.AddAttribute(3, "data-wowhead", $"item={Id}");
            }

            if (LinkEnabled)
            {
                builder.AddAttribute(4, "href", $"https://www.tbcdb.com/?item={Id}");
            }
            else
            {
                builder.AddAttribute(4, "style", "cursor: pointer");
            }

            if (ChildContent is null)
            {
                builder.AddContent(5, $"https://www.tbcdb.com/?item={Id}");
            }
            else
            {
                builder.AddContent(5, ChildContent);
            }

            builder.CloseElement();
        }
    }
}
