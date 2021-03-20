// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared
{
    public class ItemIcon : WowIcon
    {
        [Parameter] public string? Class { get; set; }
        [Parameter] public uint? ItemId { get; set; }
        [Inject] public ItemProvider Items { get; set; } = null!;

        protected Item? Item { get; set; }

        protected override string GetIconId() => Item?.Icon ?? string.Empty;

        protected override string GetAltText() => Item?.Name ?? string.Empty;

        protected override bool IconReady() => Item is not null;

        protected override async Task OnParametersSetAsync()
        {
            if (ItemId.HasValue)
            {
                Item = await Items.GetItemAsync(ItemId.Value);
            }
        }
    }
}
