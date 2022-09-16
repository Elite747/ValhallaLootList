// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters.LootList;

internal static class LootListExtensions
{
    public static int? GetCollapsibleBracket(this LootListDto lootList)
    {
        foreach (var bracket in lootList.Entries.Where(e => !e.Won).GroupBy(e => new { e.Bracket, e.Heroic }))
        {
            int highestUnselected = 0;

            foreach (var entry in bracket.OrderByDescending(e => e.Rank))
            {
                if (entry.ItemId.HasValue)
                {
                    if (highestUnselected > entry.Rank)
                    {
                        // higher rank is not filled.
                        return bracket.Key.Bracket + 1;
                    }
                }
                else
                {
                    highestUnselected = entry.Rank;
                }
            }
        }

        return null;
    }
}
