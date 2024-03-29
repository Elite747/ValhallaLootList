﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Pages.Teams.Graphs;

public class CompByTokenGraph : CompGraph
{
    protected override IList<PieChartData> GetData()
    {
        return Team.Roster
            .GroupBy(a => GetToken(a.Character.Class))
            .OrderBy(g => g.Key)
            .Select(cg => new PieChartData(cg.Count(), $"{cg.Key} ({cg.Count()})"))
            .OrderBy(data => data.Label)
            .ToList();
    }

    private static string GetToken(Classes @class)
    {
        return @class switch
        {
            Classes.Warrior or Classes.Hunter or Classes.Shaman => "Warrior/Hunter/Shaman",
            Classes.Rogue or Classes.DeathKnight or Classes.Mage or Classes.Druid => "Rogue/Death Knight/Mage/Druid",
            Classes.Paladin or Classes.Priest or Classes.Warlock => "Paladin/Priest/Warlock",
            _ => "Unknown"
        };
    }
}
