// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientDonations
{
    public ApiClientDonations(ApiClient client) => Client = client;

    public ApiClient Client { get; }

    public IApiClientOperation Add(DonationSubmissionDto donation)
    {
        return Client.CreateRequest(HttpMethod.Post, "api/v1/donations", donation);
    }
}
