// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared
{
    public class CascadingItemContext : ComponentBase
    {
        private readonly ItemLinkContext _context = new();

        [Parameter] public uint? Id { get; set; }

        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Inject] public ItemProvider Items { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            if (_context.Id != Id)
            {
                _context.Item = null;
                _context.Id = Id;
                _context.Failed = false;

                if (Id > 0)
                {
                    try
                    {
                        _context.Item = await Items.GetItemAsync(Id.Value);
                        StateHasChanged();
                    }
                    catch
                    {
                        _context.Failed = true;
                    }
                }
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingValue<ItemLinkContext>>(0);
            builder.AddAttribute(1, nameof(CascadingValue<ItemLinkContext>.Value), _context);
            builder.AddAttribute(2, nameof(CascadingValue<ItemLinkContext>.ChildContent), ChildContent);
            builder.CloseComponent();
        }
    }
}
