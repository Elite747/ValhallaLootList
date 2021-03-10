// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientDrops
    {
        public ApiClientDrops(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IList<ItemPrioDto>> GetPriorityRankings(string dropId)
        {
            return Client.CreateRequest<IList<ItemPrioDto>>(HttpMethod.Get, $"api/v1/drops/{dropId}/ranks");
        }

        public IApiClientOperation<EncounterDropDto> Assign(string dropId, string? characterId)
        {
            return Client.CreateRequest<AwardDropSubmissionDto, EncounterDropDto>(
                HttpMethod.Put,
                "api/v1/drops/" + dropId,
                new AwardDropSubmissionDto { WinnerId = characterId });
        }
    }
}
