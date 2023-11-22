// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Client.Pages.Teams.Graphs;

public class PieChartData(double data, string label, string? color = null)
{
    public double Data { get; } = data;

    public string Label { get; } = label;

    public string? Color { get; } = color;
}
