// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class RaidCalendar
    {
        private readonly List<RaidDto> _raids = new List<RaidDto>();
        private DateTime _date;

        protected override void OnParametersSet()
        {
            if (Team is null) throw new ArgumentNullException(nameof(Team));
        }

        protected override Task OnInitializedAsync()
        {
            return SetDateAsync(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));
        }

        private async Task SetDateAsync(DateTime date)
        {
            _date = date;
            _raids.Clear();
            StateHasChanged();

            try
            {
                var raids = await Api.GetAsync<List<RaidDto>>($"api/v1/raids?team={Team.Id}&y={date.Year}&m={date.Month}");

                if (raids is not null)
                {
                    _raids.AddRange(raids);
                    StateHasChanged();
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }
    }
}
