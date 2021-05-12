// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientLootListEntries
    {
        public ApiClientLootListEntries(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<LootListEntryUpdateDto> Submit(long entryId, LootListEntrySubmissionDto dto)
        {
            return Client.CreateRequest<LootListEntrySubmissionDto, LootListEntryUpdateDto>(HttpMethod.Put, $"api/v1/lootlistentries/{entryId}", dto);
        }
    }
}
