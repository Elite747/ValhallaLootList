// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Pages.Teams.Graphs;

public class CompByRoleGraph : CompGraph
{
    private readonly List<PieChartData> _data = new();

    protected override IList<PieChartData> GetData()
    {
        _data.Clear();

        int tanks = 0, heals = 0, ranged = 0, melee = 0, unknown = 0;

        foreach (var member in Team.Roster)
        {
            var list = member.LootLists.Find(ll => ll.Phase == Phase);

            if (list is null)
            {
                unknown++;
            }
            else if ((list.MainSpec & SpecializationGroups.Tank) != 0)
            {
                tanks++;
            }
            else if ((list.MainSpec & SpecializationGroups.Healer) != 0)
            {
                heals++;
            }
            else if ((list.MainSpec & SpecializationGroups.MeleeDps) != 0)
            {
                melee++;
            }
            else
            {
                ranged++;
            }
        }

        _data.Add(new(tanks, $"Tanks ({tanks})", "#001C52"));
        _data.Add(new(heals, $"Healers ({heals})", "#084D42"));
        _data.Add(new(ranged, $"Ranged DPS ({ranged})", "#6D0A5F"));
        _data.Add(new(melee, $"Melee DPS ({melee})", "#6D0A0B"));
        if (unknown != 0)
        {
            _data.Add(new(unknown, $"Unknown ({unknown})", "#000000"));
        }

        return _data;
    }
}
