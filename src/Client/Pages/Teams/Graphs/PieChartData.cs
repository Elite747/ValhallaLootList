// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Pages.Teams.Graphs
{
    public class PieChartData
    {
        public PieChartData(double data, string label, string? color = null)
        {
            Label = label;
            Color = color;
            Data = data;
        }

        public double Data { get; }

        public string Label { get; }

        public string? Color { get; }
    }
}
