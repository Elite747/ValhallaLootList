﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Xml.Linq;
using ValhallaLootList.Client.Data.Containers;

namespace ValhallaLootList.Client.Data;

public sealed class AzureClient(IHttpClientFactory httpClientFactory) : IDisposable
{
    public const string HttpClientKey = "AzureAPI";
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient(HttpClientKey);
    private readonly string _domain = "valhallalootliststorage.blob.core.windows.net";
    private bool _disposedValue;

    public string GetDomain()
    {
        return _domain;
    }

    public async Task<AzureContainerResponse> GetContainerAsync(string container, CancellationToken cancellationToken = default)
    {
        await using var stream = await _httpClient.GetStreamAsync($"https://{_domain}/{container}?restype=container&comp=list", cancellationToken);

        var document = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

        if (document.Root is null)
        {
            throw new Exception("Invalid Xml Response");
        }

        var containerName = document.Root.Attribute("ContainerName")?.Value ?? container;
        var blobs = new List<Blob>();

        var blobElements = document.Root.Element("Blobs")?.Elements();

        if (blobElements is not null)
        {
            foreach (var element in blobElements)
            {
                var name = element.Element("Name")?.Value;
                var url = element.Element("Url")?.Value;

                if (name?.Length > 0 && url?.Length > 0)
                {
                    blobs.Add(new(name, url));
                }
            }
        }

        return new(containerName, blobs);
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
