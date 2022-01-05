// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Client.Shared;

public class GameCurrencyResult
{
    [Range(0, 10000)]
    public int? Gold { get; set; }

    [Range(0, 99)]
    public int? Silver { get; set; }

    [Range(0, 99)]
    public int? Copper { get; set; }

    public bool ApplyThisMonth { get; set; }

    public int GetAmount()
    {
        int amount = Copper.GetValueOrDefault();

        if (Silver.HasValue)
        {
            amount += (Silver.Value * 100);
        }

        if (Gold.HasValue)
        {
            amount += (Gold.Value * 100 * 100);
        }

        return amount;
    }
}
