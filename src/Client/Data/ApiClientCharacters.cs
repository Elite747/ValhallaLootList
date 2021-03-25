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

        public IApiClientOperation<IList<CharacterDto>> GetByTeam(long team)
        {
            return Client.CreateRequest<IList<CharacterDto>>(HttpMethod.Get, "api/v1/characters?team=" + team);
        }

        public IApiClientOperation<IList<CharacterDto>> GetUnrostered()
        {
            return Client.CreateRequest<IList<CharacterDto>>(HttpMethod.Get, "api/v1/characters?team=none");
        }

        public IApiClientOperation<IList<CharacterDto>> GetOwned()
        {
            return Client.CreateRequest<IList<CharacterDto>>(HttpMethod.Get, "api/v1/characters?owned=true");
        }

        public IApiClientOperation<CharacterDto> Get(long id)
        {
            return Client.CreateRequest<CharacterDto>(HttpMethod.Get, "api/v1/characters/" + id);
        }

        public IApiClientOperation<CharacterDto> Get(string name)
        {
            return Client.CreateRequest<CharacterDto>(HttpMethod.Get, "api/v1/characters/byname/" + name);
        }

        public IApiClientOperation<CharacterDto> Update(long id, CharacterSubmissionDto character)
        {
            return Client.CreateRequest<CharacterSubmissionDto, CharacterDto>(HttpMethod.Put, "api/v1/characters/" + id, character);
        }

        public IApiClientOperation Delete(long id)
        {
            return Client.CreateRequest(HttpMethod.Delete, "api/v1/characters/" + id);
        }

        public IApiClientOperation<CharacterOwnerDto> GetOwner(long id)
        {
            return Client.CreateRequest<CharacterOwnerDto>(HttpMethod.Get, "api/v1/characters/" + id + "/owner");
        }

        public IApiClientOperation SetOwner(long id, long ownerId)
        {
            return Client.CreateRequest(HttpMethod.Put, "api/v1/characters/" + id + "/ownerid", ownerId);
        }

        public IApiClientOperation DeleteOwner(long id)
        {
            return Client.CreateRequest(HttpMethod.Delete, "api/v1/characters/" + id + "/ownerid");
        }

        public IApiClientOperation VerifyOwner(long id)
        {
            return Client.CreateRequest(HttpMethod.Post, "api/v1/characters/" + id + "/verify");
        }
        public IApiClientOperation<IList<CharacterAttendanceDto>> GetAttendances(long id)
        {
            return Client.CreateRequest<IList<CharacterAttendanceDto>>(HttpMethod.Get, "api/v1/characters/" + id + "/attendances");
        }

        public IApiClientOperation<CharacterDto> Create(CharacterSubmissionDto character)
        {
            return Client.CreateRequest<CharacterSubmissionDto, CharacterDto>(HttpMethod.Post, "api/v1/characters", character);
        }
    }
}
