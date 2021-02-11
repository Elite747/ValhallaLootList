// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public class RestrictionStatus : ComponentBase
    {
        [Parameter] public IEnumerable<RestrictionDto> Restrictions { get; set; } = null!;
        [Parameter] public string? SpecPrefix { get; set; }

        protected override void OnParametersSet()
        {
            if (Restrictions is null) throw new ArgumentNullException(nameof(Restrictions));
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(1, "span");

            ItemRestrictionLevel? level = null;

            foreach (var restriciton in Restrictions)
            {
                if ((level = restriciton.Level) == ItemRestrictionLevel.Restricted)
                {
                    break;
                }
            }

            string content;

            switch (level)
            {
                case ItemRestrictionLevel.ManualReview:
                    builder.AddAttribute(2, "class", "badge badge-warning");
                    content = "possibly restricted";
                    break;
                case null:
                    builder.AddAttribute(2, "class", "badge badge-success");
                    content = "allowed";
                    break;
                default:
                    builder.AddAttribute(2, "class", "badge badge-danger");
                    content = "restricted";
                    break;
            }

            if (SpecPrefix?.Length > 0)
            {
                content = SpecPrefix + ": " + content;
            }

            builder.AddContent(3, content);

            builder.CloseElement();
        }
    }
}
