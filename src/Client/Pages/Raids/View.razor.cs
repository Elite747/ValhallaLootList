// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class View
    {
        private RaidDto? _raid;
        private bool _notFound;

        protected override Task OnParametersSetAsync()
        {
            return RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            _raid = null;
            if (!string.IsNullOrWhiteSpace(RaidId))
            {
                try
                {
                    _raid = await Api.GetAsync<RaidDto>("api/v1/raids/" + RaidId);
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
                catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
                {
                    _notFound = true;
                }
            }
        }
    }
}
