// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public partial class ApiClient
    {
        public IApiClientOperation<PhaseConfigDto> GetPhaseConfiguration()
        {
            return CreateRequest<PhaseConfigDto>(HttpMethod.Get, "api/v1/config/phases").CacheFor(TimeSpan.FromHours(2));
        }
    }
}
