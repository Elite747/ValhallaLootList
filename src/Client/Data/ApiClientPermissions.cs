﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientPermissions
{
    public ApiClientPermissions(ApiClient client)
    {
        Client = client;
    }

    public ApiClient Client { get; }

    public IApiClientOperation<PermissionsDto> Get()
    {
        return Client.CreateRequest<PermissionsDto>(HttpMethod.Get, "api/v1/permissions");
    }
}
