// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientPhases
{
    public ApiClientPhases(ApiClient client)
    {
        Client = client;
    }

    public ApiClient Client { get; }

    public IApiClientOperation<List<PhaseDto>> GetAll()
    {
        return Client.CreateRequest<List<PhaseDto>>(HttpMethod.Get, "api/v1/phases");
    }

    public IApiClientOperation<PhaseDto> Get(int id)
    {
        return Client.CreateRequest<PhaseDto>(HttpMethod.Get, $"api/v1/phases/{id}");
    }

    public IApiClientOperation<PhaseDto> Create(PhaseDto phase)
    {
        return Client.CreateRequest<PhaseDto, PhaseDto>(HttpMethod.Post, "api/v1/phases", phase);
    }

    public IApiClientOperation<PhaseDto> Update(PhaseDto phase)
    {
        return Client.CreateRequest<PhaseDto, PhaseDto>(HttpMethod.Put, $"api/v1/phases/{phase.Phase}", phase);
    }

    public IApiClientOperation Delete(int id)
    {
        return Client.CreateRequest(HttpMethod.Delete, $"api/v1/phases/{id}");
    }
}
