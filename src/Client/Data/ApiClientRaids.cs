﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientRaids(ApiClient client)
{
    public ApiClient Client { get; } = client;

    public IApiClientOperation<IList<RaidDto>> GetRecent()
    {
        return Client.CreateRequest<IList<RaidDto>>(HttpMethod.Get, "api/v1/raids");
    }

    public IApiClientOperation<IList<RaidDto>> GetMine()
    {
        return Client.CreateRequest<IList<RaidDto>>(HttpMethod.Get, "api/v1/raids/@mine");
    }

    public IApiClientOperation<IList<RaidDto>> GetRecentForTeam(long teamId)
    {
        return Client.CreateRequest<IList<RaidDto>>(HttpMethod.Get, $"api/v1/raids?team={teamId}");
    }

    public IApiClientOperation<IList<RaidDto>> GetForMonth(int year, int month, long? teamId = null)
    {
        return Client.CreateRequest<IList<RaidDto>>(HttpMethod.Get, $"api/v1/raids?team={teamId}&y={year}&m={month}");
    }

    public IApiClientOperation<RaidDto> Get(long raidId)
    {
        return Client.CreateRequest<RaidDto>(HttpMethod.Get, "api/v1/raids/" + raidId);
    }

    public IApiClientOperation<RaidDto> Create(RaidSubmissionDto submission)
    {
        return Client.CreateRequest<RaidSubmissionDto, RaidDto>(HttpMethod.Post, "api/v1/raids", submission);
    }

    public IApiClientOperation Delete(long raidId)
    {
        return Client.CreateRequest(HttpMethod.Delete, "api/v1/raids/" + raidId);
    }

    public IApiClientOperation Delete(long raidId, string encounterId, byte trashIndex)
    {
        return Client.CreateRequest(HttpMethod.Delete, $"api/v1/raids/{raidId}/Kills/{encounterId}/{trashIndex}");
    }

    public IApiClientOperation Unlock(RaidDto raid)
    {
        var request = Client.CreateRequest<UnlockResponse>(HttpMethod.Post, $"api/v1/raids/{raid.Id}/unlock");
        request.ConfigureSuccess(response => raid.LocksAt = response.LocksAt);
        return request;
    }

    public IApiClientOperation<AttendanceDto> AddAttendee(long raidId, long characterId, bool standby, string? ignoreReason = null)
    {
        return Client.CreateRequest<AttendeeSubmissionDto, AttendanceDto>(
            HttpMethod.Post,
            $"api/v1/raids/{raidId}/attendees",
            new() { CharacterId = characterId, Standby = standby, IgnoreAttendance = ignoreReason?.Length > 0, IgnoreReason = ignoreReason });
    }

    public IApiClientOperation<AttendanceDto> UpdateAttendee(long raidId, long characterId, UpdateAttendanceSubmissionDto dto)
    {
        return Client.CreateRequest<UpdateAttendanceSubmissionDto, AttendanceDto>(HttpMethod.Put, $"api/v1/raids/{raidId}/attendees/{characterId}", dto);
    }

    public IApiClientOperation RemoveAttendee(long raidId, long characterId)
    {
        return Client.CreateRequest(HttpMethod.Delete, $"api/v1/raids/{raidId}/attendees/{characterId}");
    }

    public IApiClientOperation<EncounterKillDto> AddKill(long raidId, KillSubmissionDto killSubmission)
    {
        return Client.CreateRequest<KillSubmissionDto, EncounterKillDto>(HttpMethod.Post, $"api/v1/raids/{raidId}/kills", killSubmission);
    }

    public IApiClientOperation<EncounterKillDto> ModifyKill(long raidId, byte trashIndex, KillSubmissionDto kill)
    {
        return Client.CreateRequest<KillSubmissionDto, EncounterKillDto>(HttpMethod.Put, $"api/v1/raids/{raidId}/kills/{kill.EncounterId}/{trashIndex}", kill);
    }
}
