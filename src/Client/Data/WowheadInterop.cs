// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Data
{
    public class WowheadInterop
    {
        private readonly IJSRuntime _js;

        public WowheadInterop(IJSRuntime js)
        {
            _js = js;
        }

        public ValueTask HideTooltipAsync(CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("WH.Tooltip.hide", cancellationToken);
        }

        public ValueTask<int> GetLocaleFromDomainAsync(string domain, CancellationToken cancellationToken = default)
        {
            return _js.InvokeAsync<int>("WH.getLocaleFromDomain", cancellationToken, domain);
        }

        public ValueTask<int> GetTypeIdFromTypeStringAsync(string type, CancellationToken cancellationToken = default)
        {
            return _js.InvokeAsync<int>("WH.getTypeIdFromTypeString", cancellationToken, type);
        }

        public ValueTask<int> GetDataEnvFromTermAsync(string dataEnv, CancellationToken cancellationToken = default)
        {
            return _js.InvokeAsync<int>("WH.getDataEnvFromTerm", cancellationToken, dataEnv);
        }

        public ValueTask RegisterEntityAsync(int type, string id, int env, int locale, object item, CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("$WowheadPower.register", cancellationToken, type, id, env, locale, item);
        }

        public async ValueTask<object?> GetEntityAsync(int type, string id, int env, int locale, CancellationToken cancellationToken = default)
        {
            var rootElement = await _js.InvokeAsync<JsonElement>("$WowheadPower.getEntity", cancellationToken, type, id, env, locale);

            if (rootElement.TryGetProperty("data", out var dataElement))
            {
                if (dataElement.TryGetProperty("name", out var nameElement) &&
                    dataElement.TryGetProperty("quality", out var qualityElement) &&
                    dataElement.TryGetProperty("icon", out var iconElement) &&
                    dataElement.TryGetProperty("tooltip", out var tooltipElement))
                {
                    return new WowheadItemResponse
                    {
                        Icon = iconElement.GetString() ?? string.Empty,
                        Name = nameElement.GetString() ?? string.Empty,
                        Quality = qualityElement.GetInt32(),
                        Tooltip = tooltipElement.GetString() ?? string.Empty
                    };
                }

                if (dataElement.TryGetProperty("error", out var errorElement))
                {
                    return new WowheadErrorResponse { Error = errorElement.GetString() };
                }

                return dataElement.Clone();
            }

            return rootElement.Clone();
        }
    }
}
