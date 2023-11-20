// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientLootListEntries(ApiClient client)
{
    public ApiClient Client { get; } = client;

    public IApiClientOperation<LootListEntryUpdateDto> Submit(long entryId, LootListEntrySubmissionDto dto)
    {
        return Client.CreateRequest<LootListEntrySubmissionDto, LootListEntryUpdateDto>(HttpMethod.Put, $"api/v1/lootlistentries/{entryId}", dto);
    }

    public IApiClientOperation AutoPass(long entryId, bool autoPass)
    {
        return Client.CreateRequest(HttpMethod.Post, $"api/v1/lootlistentries/{entryId}/autopass?autoPass={autoPass}");
    }
}
