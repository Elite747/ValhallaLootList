// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientRaids
    {
        public ApiClientRaids(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<IList<RaidDto>> Get(string teamId, int year, int month)
        {
            return Client.CreateRequest<IList<RaidDto>>(HttpMethod.Get, $"api/v1/raids?team={teamId}&y={year}&m={month}");
        }

        public IApiClientOperation<RaidDto> Get(string raidId)
        {
            return Client.CreateRequest<RaidDto>(HttpMethod.Get, "api/v1/raids/" + raidId);
        }

        public IApiClientOperation<RaidDto> Create(RaidSubmissionDto submission)
        {
            return Client.CreateRequest<RaidSubmissionDto, RaidDto>(HttpMethod.Post, "api/v1/raids", submission);
        }

        public IApiClientOperation Delete(string raidId, string encounterId)
        {
            return Client.CreateRequest(HttpMethod.Delete, $"api/v1/raids/{raidId}/Kills/{encounterId}");
        }

        public IApiClientOperation AddAttendee(string raidId, string characterId)
        {
            return Client.CreateRequest<AttendeeSubmissionDto>(HttpMethod.Post, $"api/v1/raids/{raidId}/attendees", new() { CharacterId = characterId });
        }

        public IApiClientOperation RemoveAttendee(string raidId, string characterId)
        {
            return Client.CreateRequest<AttendeeSubmissionDto>(HttpMethod.Delete, $"api/v1/raids/{raidId}/attendees/{characterId}");
        }

        public IApiClientOperation<EncounterKillDto> AddKill(string raidId, KillSubmissionDto killSubmission)
        {
            return Client.CreateRequest<KillSubmissionDto, EncounterKillDto>(HttpMethod.Post, $"api/v1/raids/{raidId}/kills", killSubmission);
        }
    }
}
