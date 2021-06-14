// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using ValhallaLootList.Client.Data.Containers;

namespace ValhallaLootList.Client.Data
{
    public sealed class AzureClient : IDisposable
    {
        public const string HttpClientKey = "AzureAPI";
        private readonly HttpClient _httpClient;
        private readonly string _domain;
        private bool _disposedValue;

        public AzureClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient(HttpClientKey);
            _domain = "valhallalootliststorage.blob.core.windows.net";
        }

        public string GetDomain() => _domain;

        public async Task<object?> GetContainerAsync(string container, CancellationToken cancellationToken = default)
        {
            using var response = await _httpClient.GetAsync($"https://{_domain}/{container}?restype=container&comp=list", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var str = await response.Content.ReadAsStringAsync(cancellationToken);
                var buffer = Encoding.UTF8.GetBytes(str);
                using var stream = new MemoryStream(buffer);
                var serializer = new XmlSerializer(typeof(AzureContainerResponse));
                var azureContainerResponse = serializer.Deserialize(stream) as AzureContainerResponse;

                Debug.Assert(azureContainerResponse is not null);
                Debug.Assert(azureContainerResponse.Blobs is not null);
                foreach(Blob blob in azureContainerResponse.Blobs)
                {
                    Debug.Assert(blob.name is not null);
                    Debug.Assert(blob.url is not null);
                }

                azureContainerResponse.ContainerName = container;

                return azureContainerResponse;
            }

            return await response.Content.ReadAsStringAsync(cancellationToken);
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
