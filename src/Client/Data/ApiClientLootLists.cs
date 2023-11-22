// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientLootLists(ApiClient client)
{
    public ApiClient Client { get; } = client;

    public IApiClientOperation<string> GetStandings(long teamId)
    {
        return Client.CreateRequest<string>(HttpMethod.Get, $"api/v1/lootlists/standings?teamId={teamId}");
    }

    public IApiClientOperation<List<LootListDto>> GetForCharacter(long id)
    {
        return Client.CreateRequest<List<LootListDto>>(HttpMethod.Get, $"api/v1/lootlists?characterId={id}");
    }

    public IApiClientOperation<List<LootListDto>> GetForTeam(long id, bool includeApplicants = false)
    {
        return Client.CreateRequest<List<LootListDto>>(HttpMethod.Get, $"api/v1/lootlists?teamId={id}&includeApplicants={includeApplicants}");
    }

    public IApiClientOperation<LootListDto> Create(LootListSubmissionDto submission)
    {
        return Client.CreateRequest<LootListSubmissionDto, LootListDto>(HttpMethod.Post, "api/v1/lootlists", submission);
    }

    public IApiClientOperation SetSpec(LootListSubmissionDto submission)
    {
        return Client.CreateRequest(HttpMethod.Put, "api/v1/lootlists", submission);
    }

    public IApiClientOperation<MultiTimestampDto> SubmitAll(List<LootListDto> lootLists, List<long> submitTo, byte size)
    {
        var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
        var request = Client.CreateRequest<SubmitAllListsDto, MultiTimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{characterId}/submitAll",
            new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = size, SubmitTo = submitTo });

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

    public IApiClientOperation<MultiTimestampDto> RevokeAll(List<LootListDto> lootLists, byte size)
    {
        var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
        var request = Client.CreateRequest<MultiTimestampDto, MultiTimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{characterId}/revokeall",
            new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = size });

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

    public IApiClientOperation<ApproveAllListsResponseDto> ApproveAll(List<LootListDto> lootLists, TeamDto team, string? message, bool bench)
    {
        var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
        var request = Client.CreateRequest<ApproveOrRejectAllListsDto, ApproveAllListsResponseDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{characterId}/approveall/{team.Id}",
            new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Message = message, Bench = bench, Size = team.Size });

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

    public IApiClientOperation<MultiTimestampDto> RejectAll(List<LootListDto> lootLists, TeamDto team, string? message)
    {
        var characterId = lootLists.Select(ll => ll.CharacterId).Distinct().Single();
        var request = Client.CreateRequest<ApproveOrRejectAllListsDto, MultiTimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{characterId}/rejectall/{team.Id}",
            new() { Timestamps = lootLists.ToDictionary(ll => ll.Phase, ll => ll.Timestamp), Size = team.Size, Message = message });

        request.ConfigureSuccess(response =>
        {
            foreach (var lootList in lootLists)
            {
                lootList.Timestamp = response.Timestamps[lootList.Phase];
                if (lootList.Status != LootListStatus.Locked)
                {
                    lootList.Status = LootListStatus.Editing;
                }
                lootList.SubmittedTo.Remove(team.Id);
            }
        });

        return request;
    }

    public IApiClientOperation<TimestampDto> Submit(LootListDto lootList)
    {
        var request = Client.CreateRequest<LootListActionDto, TimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{lootList.CharacterId}/submit",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size });

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
        var request = Client.CreateRequest<LootListActionDto, TimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{lootList.CharacterId}/revoke",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size });

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
            $"api/v1/lootlists/{lootList.CharacterId}/approve",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size, Message = message });

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
            $"api/v1/lootlists/{lootList.CharacterId}/reject",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size, Message = message });

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
        var request = Client.CreateRequest<LootListActionDto, TimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{lootList.CharacterId}/lock",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size });

        request.ConfigureSuccess(response =>
        {
            lootList.Timestamp = response.Timestamp;
            lootList.Status = LootListStatus.Locked;
        });

        return request;
    }

    public IApiClientOperation<TimestampDto> Unlock(LootListDto lootList)
    {
        var request = Client.CreateRequest<LootListActionDto, TimestampDto>(
            HttpMethod.Post,
            $"api/v1/lootlists/{lootList.CharacterId}/unlock",
            new() { Timestamp = lootList.Timestamp, Phase = lootList.Phase, Size = lootList.Size });

        request.ConfigureSuccess(response =>
        {
            lootList.Timestamp = response.Timestamp;
            lootList.Status = LootListStatus.Approved;
        });

        return request;
    }
}
