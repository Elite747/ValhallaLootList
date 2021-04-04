// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientLootLists
    {
        public ApiClientLootLists(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IList<LootListDto>> GetForCharacter(long id, byte? phase = null)
        {
            string path = "api/v1/lootlists?characterId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            return Client.CreateRequest<IList<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<IList<LootListDto>> GetForTeam(long id, byte? phase = null)
        {
            string path = "api/v1/lootlists?teamId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            return Client.CreateRequest<IList<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<LootListDto> Create(long characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation<LootListDto> Reset(long characterId, byte phase)
        {
            return Client.CreateRequest<LootListDto>(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}/reset");
        }

        public IApiClientOperation SetSpec(long characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Put, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation SetStatus(long characterId, byte phase, SetLootListStatusDto dto)
        {
            return Client.CreateRequest(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}/status", dto);
        }
    }
}
