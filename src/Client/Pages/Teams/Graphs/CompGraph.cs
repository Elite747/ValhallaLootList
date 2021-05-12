// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.AspNetCore.Components;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams.Graphs
{
    public abstract class CompGraph : SimplePieChart
    {
        [Parameter] public TeamDto Team { get; set; } = null!;

        [Parameter] public byte Phase { get; set; }

        protected override void OnParametersSet()
        {
            if (Team is null) throw new ArgumentNullException(nameof(Team));
            base.OnParametersSet();
        }
    }
}
