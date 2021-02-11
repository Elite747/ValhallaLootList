// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ValhallaLootList.Client
{
    public sealed class ApiClient : IDisposable
    {
        public const string HttpClientKey = "ValhallaLootList.ServerAPI";
        private readonly HttpClient _httpClient;
        private bool _disposedValue;

        public ApiClient(IHttpClientFactory httpClientFactory, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientKey);
            JsonSerializerOptions = jsonOptions.Value;
        }

        public JsonSerializerOptions JsonSerializerOptions { get; }

        public async Task<TValue?> GetAsync<TValue>(string requestUri, CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri), cancellationToken);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TValue>(JsonSerializerOptions, cancellationToken).ConfigureAwait(false);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return SendAsync(new HttpRequestMessage(HttpMethod.Post, requestUri), cancellationToken);
        }

        public Task<HttpResponseMessage> PostAsync<TValue>(string requestUri, TValue value, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
            {
                Content = JsonContent.Create(value, mediaType: null, JsonSerializerOptions)
            };

            return SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> PutAsync<TValue>(string requestUri, TValue value, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri)
            {
                Content = JsonContent.Create(value, mediaType: null, JsonSerializerOptions)
            };

            return SendAsync(request, cancellationToken);
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            return SendAsync(new HttpRequestMessage(HttpMethod.Delete, requestUri), cancellationToken);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
        {
            return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
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
