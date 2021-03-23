// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientTeams
    {
        public ApiClientTeams(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IList<string>> GetAllTeamNames()
        {
            return Client.CreateRequest<IList<string>>(HttpMethod.Get, "api/v1/teams");
        }

        public IApiClientOperation<TeamDto> Get(string teamId)
        {
            return Client.CreateRequest<TeamDto>(HttpMethod.Get, "api/v1/teams/" + teamId);
        }

        public IApiClientOperation<TeamDto> Create(TeamSubmissionDto submission)
        {
            return Client.CreateRequest<TeamSubmissionDto, TeamDto>(HttpMethod.Post, "api/v1/teams", submission);
        }

        public IApiClientOperation<TeamCharacterDto> AddMember(string teamId, AddTeamMemberDto dto)
        {
            return Client.CreateRequest<AddTeamMemberDto, TeamCharacterDto>(HttpMethod.Post, $"api/v1/teams/{teamId}/members", dto);
        }

        public IApiClientOperation UpdateMember(string teamId, string characterId, UpdateTeamMemberDto dto)
        {
            return Client.CreateRequest(HttpMethod.Put, $"api/v1/teams/{teamId}/members/{characterId}", dto);
        }

        public IApiClientOperation RemoveMember(string teamId, string characterId)
        {
            return Client.CreateRequest(HttpMethod.Delete, $"api/v1/teams/{teamId}/members/{characterId}");
        }

        public IApiClientOperation<IList<GuildMemberDto>> GetLeaders(string teamId)
        {
            return Client.CreateRequest<IList<GuildMemberDto>>(HttpMethod.Get, "api/v1/teams/" + teamId + "/leaders");
        }

        public IApiClientOperation<GuildMemberDto> AddLeader(string teamId, string userId)
        {
            return Client.CreateRequest<GuildMemberDto>(HttpMethod.Post, "api/v1/teams/" + teamId + "/leaders/" + userId);
        }

        public IApiClientOperation RemoveLeader(string teamId, string userId)
        {
            return Client.CreateRequest(HttpMethod.Delete, "api/v1/teams/" + teamId + "/leaders/" + userId);
        }
    }
}
