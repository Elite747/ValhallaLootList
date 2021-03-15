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

        public IApiClientOperation<IList<LootListDto>> GetForCharacter(string id, byte? phase = null)
        {
            string path = "api/v1/lootlists?characterId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            return Client.CreateRequest<IList<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<IList<LootListDto>> GetForTeam(string id, byte? phase = null)
        {
            string path = "api/v1/lootlists?teamId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            return Client.CreateRequest<IList<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<LootListDto> Create(string characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation<LootListDto> Recreate(string characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Put, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation Lock(string characterId, byte phase)
        {
            return Client.CreateRequest(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}/lock");
        }

        public IApiClientOperation Unlock(string characterId, byte phase)
        {
            return Client.CreateRequest(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}/unlock");
        }
    }
}
