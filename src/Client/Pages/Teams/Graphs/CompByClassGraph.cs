// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;

namespace ValhallaLootList.Client.Pages.Teams.Graphs
{
    public class CompByClassGraph : CompGraph
    {
        protected override IList<PieChartData> GetData()
        {
            return Team.Roster
                .GroupBy(a => a.Character.Class)
                .Select(cg => new PieChartData(cg.Count(), $"{cg.Key.GetDisplayName()} ({cg.Count()})", cg.Key.GetClassColor()))
                .OrderBy(data => data.Label)
                .ToList();
        }
    }
}
