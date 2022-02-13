// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientDrops
{
    public ApiClientDrops(ApiClient client) => Client = client;

    public ApiClient Client { get; }

    public IApiClientOperation<IList<WonDropDto>> GetForCharacter(long characterId)
    {
        return Client.CreateRequest<IList<WonDropDto>>(HttpMethod.Get, "api/v1/drops?characterId=" + characterId);
    }

    public IApiClientOperation<IList<ItemPrioDto>> GetPriorityRankings(long dropId)
    {
        return Client.CreateRequest<IList<ItemPrioDto>>(HttpMethod.Get, $"api/v1/drops/{dropId}/ranks");
    }

    public IApiClientOperation<EncounterDropDto> Assign(long dropId, long? characterId)
    {
        return Client.CreateRequest<AwardDropSubmissionDto, EncounterDropDto>(
            HttpMethod.Put,
            "api/v1/drops/" + dropId,
            new AwardDropSubmissionDto { WinnerId = characterId });
    }

    public IApiClientOperation<List<AuditDropDto>> Audit()
    {
        return Client.CreateRequest<List<AuditDropDto>>(HttpMethod.Get, "api/v1/drops/audit");
    }
}
