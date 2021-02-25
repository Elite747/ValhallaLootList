// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Data.Items
{
    public class ItemCache : Cache<Item, uint>
    {
        protected override uint GetKey(Item item) => item.Id;
    }
}
