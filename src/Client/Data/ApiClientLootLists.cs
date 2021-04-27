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

        public IApiClientOperation<TimestampDto> SetEditable(LootListDto lootList)
        {
            var request = Client.CreateRequest<TimestampDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/seteditable",
                new() { Timestamp = lootList.Timestamp });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = LootListStatus.Editing;
                lootList.ApprovedBy = null;
                lootList.SubmittedTo.Clear();
            });

            return request;
        }

        public IApiClientOperation<TimestampDto> Submit(LootListDto lootList, List<long> submitTo)
        {
            var request = Client.CreateRequest<SubmitLootListDto, TimestampDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/submit",
                new() { Timestamp = lootList.Timestamp, SubmitTo = submitTo });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = LootListStatus.Submitted;
                lootList.SubmittedTo = submitTo;
            });

            return request;
        }

        public IApiClientOperation<ApproveOrRejectLootListResponseDto> Approve(LootListDto lootList, long teamId, string? message)
        {
            var request = Client.CreateRequest<ApproveOrRejectLootListDto, ApproveOrRejectLootListResponseDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/approveorreject",
                new() { Timestamp = lootList.Timestamp, Approved = true, TeamId = teamId, Message = message });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = response.LootListStatus;
                lootList.SubmittedTo.Clear();
            });

            return request;
        }

        public IApiClientOperation<ApproveOrRejectLootListResponseDto> Reject(LootListDto lootList, long teamId, string? message)
        {
            var request = Client.CreateRequest<ApproveOrRejectLootListDto, ApproveOrRejectLootListResponseDto>(
                HttpMethod.Post,
                $"api/v1/lootlists/phase{lootList.Phase}/{lootList.CharacterId}/approveorreject",
                new() { Timestamp = lootList.Timestamp, Approved = false, TeamId = teamId, Message = message });

            request.ConfigureSuccess(response =>
            {
                lootList.Timestamp = response.Timestamp;
                lootList.Status = response.LootListStatus;
                lootList.SubmittedTo.Remove(teamId);
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
