// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class ApiClientLootLists
    {
        public ApiClientLootLists(ApiClient client) => Client = client;

        public ApiClient Client { get; }

        public IApiClientOperation<List<LootListDto>> GetForCharacter(long id, byte? phase = null)
        {
            string path = "api/v1/lootlists?characterId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            return Client.CreateRequest<List<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<List<LootListDto>> GetForTeam(long id, byte? phase = null, bool? includeApplicants = null)
        {
            string path = "api/v1/lootlists?teamId=" + id;

            if (phase.HasValue)
            {
                path += "&phase=" + phase;
            }

            if (includeApplicants.HasValue)
            {
                path += "&includeApplicants=" + includeApplicants;
            }

            return Client.CreateRequest<List<LootListDto>>(HttpMethod.Get, path);
        }

        public IApiClientOperation<LootListDto> Create(long characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Post, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation SetSpec(long characterId, byte phase, LootListSubmissionDto submission)
        {
            return Client.CreateRequest(HttpMethod.Put, $"api/v1/lootlists/phase{phase}/{characterId}", submission);
        }

        public IApiClientOperation<MultiTimestampDto> SubmitAll(List<LootListDto> lootLists, List<long> submitTo)
        {
            var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
            var request = Client.CreateRequest<SubmitAllListsDto, MultiTimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/{characterId}/submitAll",
                new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), SubmitTo = submitTo });

            request.ConfigureSuccess(response =>
            {
                foreach (var lootList in lootLists)
                {
                    lootList.Timestamp = response.Timestamps[lootList.Phase];
                    if (lootList.Status != LootListStatus.Locked)
                    {
                        lootList.Status = LootListStatus.Submitted;
                    }
                    lootList.SubmittedTo.Clear();
                    lootList.SubmittedTo.AddRange(submitTo);
                }
            });

            return request;
        }

        public IApiClientOperation<MultiTimestampDto> RevokeAll(List<LootListDto> lootLists)
        {
            var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
            var request = Client.CreateRequest<MultiTimestampDto, MultiTimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/{characterId}/revokeall",
                new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp) });

            request.ConfigureSuccess(response =>
            {
                foreach (var lootList in lootLists)
                {
                    lootList.Timestamp = response.Timestamps[lootList.Phase];
                    if (lootList.Status != LootListStatus.Locked)
                    {
                        lootList.Status = LootListStatus.Editing;
                    }
                    lootList.SubmittedTo.Clear();
                }
            });

            return request;
        }

        public IApiClientOperation<ApproveAllListsResponseDto> ApproveAll(List<LootListDto> lootLists, long teamId, string? message)
        {
            var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
            var request = Client.CreateRequest<ApproveOrRejectAllListsDto, ApproveAllListsResponseDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/{characterId}/approveall/{teamId}",
                new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Message = message });

            request.ConfigureSuccess(response =>
            {
                foreach (var lootList in lootLists)
                {
                    lootList.Timestamp = response.Timestamps[lootList.Phase];
                    if (lootList.Status != LootListStatus.Locked)
                    {
                        lootList.Status = LootListStatus.Approved;
                    }
                    lootList.SubmittedTo.Clear();
                }
            });

            return request;
        }

        public IApiClientOperation<MultiTimestampDto> RejectAll(List<LootListDto> lootLists, long teamId, string? message)
        {
            var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
            var request = Client.CreateRequest<ApproveOrRejectAllListsDto, MultiTimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/{characterId}/rejectall/{teamId}",
                new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Message = message });

            request.ConfigureSuccess(response =>
            {
                foreach (var lootList in lootLists)
                {
                    lootList.Timestamp = response.Timestamps[lootList.Phase];
                    if (lootList.Status != LootListStatus.Locked)
                    {
                        lootList.Status = LootListStatus.Editing;
                    }
                    lootList.SubmittedTo.Remove(teamId);
                }
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Submit(LootListDto lootList)
        {
            var request = Client.CreateRequest<TimestampDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/submit",
                new() { Timestamp = lootList.Timestamp });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                if (lootList.Status != LootListStatus.Locked)
                {
                    lootList.Status = LootListStatus.Submitted;
                }
                lootList.SubmittedTo.Clear();
                if (lootList.TeamId.HasValue)
                {
                    lootList.SubmittedTo.Add(lootList.TeamId.Value);
                }
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Revoke(LootListDto lootList)
        {
            var request = Client.CreateRequest<TimestampDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/revoke",
                new() { Timestamp = lootList.Timestamp });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                if (lootList.Status != LootListStatus.Locked)
                {
                    lootList.Status = LootListStatus.Editing;
                }
                lootList.SubmittedTo.Clear();
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Approve(LootListDto lootList, string? message)
        {
            var request = Client.CreateRequest<ApproveOrRejectLootListDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/approve",
                new() { Timestamp = lootList.Timestamp, Message = message });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                if (lootList.Status != LootListStatus.Locked)
                {
                    lootList.Status = LootListStatus.Approved;
                }
                lootList.SubmittedTo.Clear();
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Reject(LootListDto lootList, string? message)
        {
            var request = Client.CreateRequest<ApproveOrRejectLootListDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/reject",
                new() { Timestamp = lootList.Timestamp, Message = message });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                if (lootList.Status != LootListStatus.Locked)
                {
                    lootList.Status = LootListStatus.Editing;
                }
                lootList.SubmittedTo.Clear();
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Lock(LootListDto lootList)
        {
            var request = Client.CreateRequest<TimestampDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/lock",
                new() { Timestamp = lootList.Timestamp });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = LootListStatus.Locked;
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Unlock(LootListDto lootList)
        {
            var request = Client.CreateRequest<TimestampDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/unlock",
                new() { Timestamp = lootList.Timestamp });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = LootListStatus.Approved;
            });

            return request;
        }
    }
}
