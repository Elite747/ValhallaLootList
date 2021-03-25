// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Data
{
    public sealed class WowheadClient : IDisposable
    {
        public const string HttpClientKey = "WowheadAPI";
        private readonly HttpClient _httpClient;
        private readonly string _domain;
        private bool _disposedValue;

        public WowheadClient(IHttpClientFactory httpClientFactory, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientKey);
            _domain = "tbc";
            JsonSerializerOptions = jsonOptions.Value;
        }

        public JsonSerializerOptions JsonSerializerOptions { get; }

        public string GetDomain() => _domain;

        public async Task<object?> GetItemAsync(uint id, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"https://{_domain}.wowhead.com/tooltip/item/{id}", cancellationToken);

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
}
