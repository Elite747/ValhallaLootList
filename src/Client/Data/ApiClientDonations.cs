// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class ApiClientDonations
{
    public ApiClientDonations(ApiClient client)
    {
        Client = client;
    }

    public ApiClient Client { get; }

    public IApiClientOperation<List<DonationDto>> GetForMonth(int month, int year)
    {
        return Client.CreateRequest<List<DonationDto>>(HttpMethod.Get, $"api/v1/donations?month={month}&year={year}");
    }

    public IApiClientOperation<DonationDto> Add(DonationSubmissionDto donation)
    {
        return Client.CreateRequest<DonationSubmissionDto, DonationDto>(HttpMethod.Post, "api/v1/donations", donation);
    }

    public IApiClientOperation<List<DonationDto>> Import(DonationImportDto import, bool skipExcess = false)
    {
        return Client.CreateRequest<DonationImportDto, List<DonationDto>>(HttpMethod.Post, $"api/v1/donations/import?skipExcess={skipExcess}", import);
    }

    public IApiClientOperation Delete(long donationId)
    {
        return Client.CreateRequest(HttpMethod.Delete, $"api/v1/donations/{donationId}");
    }
}
