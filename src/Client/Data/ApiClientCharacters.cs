// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public sealed class ApiClientCharacters
    {
        public ApiClientCharacters(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IList<CharacterDto>> GetAll()
        {
            return Client.CreateRequest<IList<CharacterDto>>(HttpMethod.Get, "api/v1/characters");
        }

        public IApiClientOperation<List<CharacterDto>> GetByTeam(string team)
        {
            return Client.CreateRequest<List<CharacterDto>>(HttpMethod.Get, "api/v1/characters?team=" + team);
        }

        public IApiClientOperation<List<CharacterDto>> GetUnrostered()
        {
            return Client.CreateRequest<List<CharacterDto>>(HttpMethod.Get, "api/v1/characters?team=none");
        }

        public IApiClientOperation<CharacterDto> Get(string id)
        {
            return Client.CreateRequest<CharacterDto>(HttpMethod.Get, "api/v1/characters/" + id);
        }

        public IApiClientOperation<CharacterDto> Create(CharacterSubmissionDto character)
        {
            return Client.CreateRequest<CharacterSubmissionDto, CharacterDto>(HttpMethod.Post, "api/v1/characters", character);
        }
    }
}
