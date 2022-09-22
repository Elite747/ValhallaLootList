// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Pages.Teams.Graphs;

public class CompByArmorGraph : CompGraph
{
    protected override IList<PieChartData> GetData()
    {
        return Team.Roster
            .GroupBy(a => GetArmorWeight(a.Character.Class))
            .OrderBy(g => g.Key)
            .Select(cg => new PieChartData(cg.Count(), $"{cg.Key} ({cg.Count()})"))
            .ToList();
    }

    private static Weight GetArmorWeight(Classes @class)
    {
        return @class switch
        {
            Classes.Warrior or Classes.Paladin or Classes.DeathKnight => Weight.Plate,
            Classes.Hunter or Classes.Shaman => Weight.Mail,
            Classes.Rogue or Classes.Druid => Weight.Leather,
            Classes.Priest or Classes.Mage or Classes.Warlock => Weight.Cloth,
            _ => Weight.Unknown
        };
    }

    private enum Weight
    {
        Cloth,
        Leather,
        Mail,
        Plate,
        Unknown
    }
}
