// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.Client.Pages.Teams.Graphs
{
    public class CompByPositionGraph : CompGraph
    {
        private readonly List<PieChartData> _data = new();

        protected override IList<PieChartData> GetData()
        {
            _data.Clear();

            int melee = 0, ranged = 0, unknown = 0;

            foreach (var member in Team.Roster)
            {
                var list = member.LootLists.Find(ll => ll.Phase == Phase);

                if (list is null)
                {
                    unknown++;
                }
                else
                {
                    switch (list.MainSpec)
                    {
                        case Specializations.BalanceDruid:
                        case Specializations.BeastMasterHunter:
                        case Specializations.ArcaneMage:
                        case Specializations.ShadowPriest:
                        case Specializations.EleShaman:
                        case Specializations.AfflictionWarlock:
                        case Specializations.MarksmanHunter:
                        case Specializations.SurvivalHunter:
                        case Specializations.FireMage:
                        case Specializations.FrostMage:
                        case Specializations.DemoWarlock:
                        case Specializations.DestroWarlock:
                            ranged++;
                            break;
                        case Specializations.BearDruid:
                        case Specializations.CatDruid:
                        case Specializations.ProtPaladin:
                        case Specializations.RetPaladin:
                        case Specializations.AssassinationRogue:
                        case Specializations.EnhanceShaman:
                        case Specializations.ProtWarrior:
                        case Specializations.ArmsWarrior:
                        case Specializations.FuryWarrior:
                        case Specializations.CombatRogue:
                        case Specializations.SubtletyRogue:
                            melee++;
                            break;
                        case Specializations.RestoDruid:
                        case Specializations.HolyPaladin:
                        case Specializations.DiscPriest:
                        case Specializations.RestoShaman:
                        case Specializations.HolyPriest:
                            break;
                        default:
                            unknown++;
                            break;
                    }
                }
            }

            _data.Add(new(melee, $"Melee ({melee})"));
            _data.Add(new(ranged, $"Ranged ({ranged})"));
            if (unknown != 0)
            {
                _data.Add(new(unknown, $"Unknown ({unknown})", "#000000"));
            }

            return _data;
        }
    }
}
