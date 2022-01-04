// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared;

public class ItemIcon : WowIcon
{
    private Item? _item;

    [Parameter] public uint? Id { get; set; }
    [Inject] public ItemProvider Items { get; set; } = null!;
    [CascadingParameter] public ItemLinkContext? Context { get; set; }

    protected override string GetIconId() => (Context?.Item ?? _item)?.Icon ?? string.Empty;

    protected override string GetAltText() => (Context?.Item ?? _item)?.Name ?? string.Empty;

    protected override bool IconReady() => (Context?.Item ?? _item) is not null;

    protected override async Task OnParametersSetAsync()
    {
        if (Context is null)
        {
            if (Id > 0)
            {
                try
                {
                    _item = await Items.GetItemAsync(Id.Value);
                }
                catch
                {
                }
            }
        }
        else
        {
            _item = Context.Item;
        }
    }
}
