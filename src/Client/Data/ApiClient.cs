// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace ValhallaLootList.Client.Data
{
    public sealed partial class ApiClient : IDisposable
    {
        public const string HttpClientKey = "ValhallaLootList.ServerAPI";
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _memoryCache;

        private bool _disposedValue;

        public ApiClient(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IOptions<JsonSerializerOptions> jsonOptions)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientKey);
            JsonSerializerOptions = jsonOptions.Value;
            _memoryCache = memoryCache;

            Characters = new(this);
            Drops = new(this);
            Instances = new(this);
            Items = new(this);
            LootLists = new(this);
            Members = new(this);
            Raids = new(this);
            Teams = new(this);
        }

        public JsonSerializerOptions JsonSerializerOptions { get; }

        public ApiClientCharacters Characters { get; }

        public ApiClientDrops Drops { get; }

        public ApiClientInstances Instances { get; }

        public ApiClientItems Items { get; }

        public ApiClientLootLists LootLists { get; }

        public ApiClientMembers Members { get; }

        public ApiClientRaids Raids { get; }

        public ApiClientTeams Teams { get; }

        public IApiClientOperation CreateRequest(HttpMethod method, string requestUri)
        {
            return CreateRequest(new HttpRequestMessage(method, requestUri));
        }

        public IApiClientOperation CreateRequest<TContent>(HttpMethod method, string requestUri, TContent content)
        {
            return CreateRequest(new HttpRequestMessage(method, requestUri)
            {
                Content = JsonContent.Create(content, mediaType: null, options: JsonSerializerOptions)
            });
        }

        public IApiClientOperation<TResult> CreateRequest<TResult>(HttpMethod method, string requestUri)
        {
            return CreateRequest<TResult>(new HttpRequestMessage(method, requestUri));
        }

        public IApiClientOperation<TResult> CreateRequest<TContent, TResult>(HttpMethod method, string requestUri, TContent content)
        {
            return CreateRequest<TResult>(new HttpRequestMessage(method, requestUri)
            {
                Content = JsonContent.Create(content, mediaType: null, options: JsonSerializerOptions)
            });
        }

        public IApiClientOperation CreateRequest(HttpRequestMessage request)
        {
            return new Operation<object>(_httpClient, _memoryCache, JsonSerializerOptions, request);
        }

        public IApiClientOperation<TResult> CreateRequest<TResult>(HttpRequestMessage request)
        {
            return new Operation<TResult>(_httpClient, _memoryCache, JsonSerializerOptions, request);
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
