// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace ValhallaLootList.Client.Shared
{
    public class ItemLinkIcon : ComponentBase
    {
        [CascadingParameter] public ItemLinkContext Context { get; set; } = null!;
        [Parameter] public string? Class { get; set; }
        [Parameter] public IconSize? Size { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Context is null)
            {
                throw new InvalidOperationException("ItemLinkIcon must be within an ItemLink.");
            }

            if (Context.Item is not null)
            {
                builder.OpenComponent<WowheadIcon>(0);

                if (Class?.Length > 0)
                {
                    builder.AddAttribute(1, "class", Class);
                }

                builder.AddAttribute(2, nameof(WowheadIcon.Size), Size ?? Context.IconSize ?? IconSize.Tiny);
                builder.AddAttribute(3, nameof(WowheadIcon.IconId), Context.Item.Icon);

                builder.CloseComponent();
            }
        }
    }
}
