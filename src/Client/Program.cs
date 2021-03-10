// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using ValhallaLootList.Client.Data;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient(ApiClient.HttpClientKey, client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddHttpClient(WowheadClient.HttpClientKey, client => client.BaseAddress = new Uri("https://www.wowhead.com/"));

            builder.Services.AddMemoryCache()
                .AddScoped<ApiClient>()
                .AddScoped<WowheadClient>()
                .AddScoped<WowheadInterop>()
                .AddScoped<ItemProvider>()
                .AddSingleton<ItemCache>();

            builder.Services.Configure<JsonSerializerOptions>(options => Serialization.SerializerOptions.ConfigureDefaultOptions(options));

            builder.Services
                .AddApiAuthorization(options =>
                {
                    options.UserOptions.NameClaim = AppClaimTypes.Name;
                    options.UserOptions.RoleClaim = AppClaimTypes.Role;
                })
                .AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();

            builder.Services.AddAuthorizationCore(AppRoles.ConfigureAuthorization);

            builder.Services.AddBlazorise(options => options.ChangeTextOnKeyPress = true).AddBootstrapProviders().AddFontAwesomeIcons();

            var host = builder.Build();

            host.Services.UseBootstrapProviders().UseFontAwesomeIcons();

            await host.RunAsync();
        }
    }
}
