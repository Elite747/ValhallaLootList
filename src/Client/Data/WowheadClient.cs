// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Data;

public sealed class WowheadClient(IHttpClientFactory httpClientFactory, IOptions<JsonSerializerOptions> jsonOptions) : IDisposable
{
    public const string HttpClientKey = "WowheadAPI";
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientKey);
    private bool _disposedValue;

    public JsonSerializerOptions JsonSerializerOptions { get; } = jsonOptions.Value;

    public async Task<object?> GetItemAsync(uint id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"https://nether.wowhead.com/tooltip/item/{id}?dataEnv=8&locale=0", cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var item = await response.Content.ReadFromJsonAsync<WowheadItemResponse>(JsonSerializerOptions, cancellationToken);

            Debug.Assert(item is not null);
            Debug.Assert(item.Name is not null);
            Debug.Assert(item.Icon is not null);
            Debug.Assert(item.Tooltip is not null);

            return item;
        }

        return await response.Content.ReadFromJsonAsync<WowheadErrorResponse>(JsonSerializerOptions, cancellationToken);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _httpClient.Dispose();
            }
            _disposedValue = true;
        }
    }

    void IDisposable.Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
