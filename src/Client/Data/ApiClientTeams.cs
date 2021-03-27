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

        public IApiClientOperation<TeamDto> Get(long teamId)
        {
            return Client.CreateRequest<TeamDto>(HttpMethod.Get, "api/v1/teams/" + teamId);
        }

        public IApiClientOperation<TeamDto> Get(string teamName)
        {
            return Client.CreateRequest<TeamDto>(HttpMethod.Get, "api/v1/teams/byname/" + teamName);
        }

        public IApiClientOperation<TeamDto> Create(TeamSubmissionDto submission)
        {
            return Client.CreateRequest<TeamSubmissionDto, TeamDto>(HttpMethod.Post, "api/v1/teams", submission);
        }

        public IApiClientOperation<TeamDto> Update(long teamId, TeamSubmissionDto submission)
        {
            return Client.CreateRequest<TeamSubmissionDto, TeamDto>(HttpMethod.Put, "api/v1/teams/" + teamId, submission);
        }

        public IApiClientOperation<MemberDto> AddMember(long teamId, AddTeamMemberDto dto)
        {
            return Client.CreateRequest<AddTeamMemberDto, MemberDto>(HttpMethod.Post, $"api/v1/teams/{teamId}/members", dto);
        }

        public IApiClientOperation UpdateMember(long teamId, long characterId, UpdateTeamMemberDto dto)
        {
            return Client.CreateRequest(HttpMethod.Put, $"api/v1/teams/{teamId}/members/{characterId}", dto);
        }

        public IApiClientOperation RemoveMember(long teamId, long characterId)
        {
            return Client.CreateRequest(HttpMethod.Delete, $"api/v1/teams/{teamId}/members/{characterId}");
        }

        public IApiClientOperation<IList<GuildMemberDto>> GetLeaders(long teamId)
        {
            return Client.CreateRequest<IList<GuildMemberDto>>(HttpMethod.Get, "api/v1/teams/" + teamId + "/leaders");
        }

        public IApiClientOperation<GuildMemberDto> AddLeader(long teamId, long userId)
        {
            return Client.CreateRequest<GuildMemberDto>(HttpMethod.Post, "api/v1/teams/" + teamId + "/leaders/" + userId);
        }

        public IApiClientOperation RemoveLeader(long teamId, long userId)
        {
            return Client.CreateRequest(HttpMethod.Delete, "api/v1/teams/" + teamId + "/leaders/" + userId);
        }
    }
}
