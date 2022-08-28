// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Data;

public class Bracket
{
    public Bracket()
    {
    }

    public Bracket(byte phase, byte index, byte minRank, byte maxRank, byte normalItems, byte heroicItems, bool allowOffspec, bool allowTypeDuplicates)
    {
        Phase = phase;
        Index = index;
        MinRank = minRank;
        MaxRank = maxRank;
        NormalItems = normalItems;
        HeroicItems = heroicItems;
        AllowOffspec = allowOffspec;
        AllowTypeDuplicates = allowTypeDuplicates;
    }

    public byte Phase { get; set; }

    public byte Index { get; set; }

    public byte MinRank { get; set; }

    public byte MaxRank { get; set; }

    public byte NormalItems { get; set; }

    public byte HeroicItems { get; set; }

    public bool AllowOffspec { get; set; }

    public bool AllowTypeDuplicates { get; set; }
}
