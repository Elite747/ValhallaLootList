// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.Client.Pages.Teams.Graphs
{
    public class CompByDamageTypeGraph : CompGraph
    {
        private readonly List<PieChartData> _data = new();

        protected override IList<PieChartData> GetData()
        {
            _data.Clear();

            int physical = 0, magic = 0, both = 0, unknown = 0;

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
                        case Specializations.ArcaneMage:
                        case Specializations.ShadowPriest:
                        case Specializations.EleShaman:
                        case Specializations.AfflictionWarlock:
                        case Specializations.FireMage:
                        case Specializations.FrostMage:
                        case Specializations.DemoWarlock:
                        case Specializations.DestroWarlock:
                            magic++;
                            break;
                        case Specializations.BearDruid:
                        case Specializations.CatDruid:
                        case Specializations.BeastMasterHunter:
                        case Specializations.AssassinationRogue:
                        case Specializations.ProtWarrior:
                        case Specializations.ArmsWarrior:
                        case Specializations.FuryWarrior:
                        case Specializations.MarksmanHunter:
                        case Specializations.SurvivalHunter:
                        case Specializations.CombatRogue:
                        case Specializations.SubtletyRogue:
                            physical++;
                            break;
                        case Specializations.RestoDruid:
                        case Specializations.HolyPaladin:
                        case Specializations.DiscPriest:
                        case Specializations.RestoShaman:
                        case Specializations.HolyPriest:
                            break;
                        case Specializations.ProtPaladin:
                        case Specializations.RetPaladin:
                        case Specializations.EnhanceShaman:
                            both++;
                            break;
                        default:
                            unknown++;
                            break;
                    }
                }
            }

            _data.Add(new(physical, $"Physical ({physical})"));
            _data.Add(new(magic, $"Magical ({magic})"));
            _data.Add(new(both, $"Both ({both})"));
            if (unknown != 0)
            {
                _data.Add(new(unknown, $"Unknown ({unknown})", "#000000"));
            }

            return _data;
        }
    }
}
