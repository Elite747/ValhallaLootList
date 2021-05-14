// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using ValhallaLootList.Client.Data;
using ValhallaLootList.Client.Data.Containers;
using ValhallaLootList.Client.Data.Items;
using ValhallaLootList.Client.Shared;

namespace ValhallaLootList.Client
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient(
                ApiClient.HttpClientKey,
                client =>
                {
                    var baseAddressBuilder = new UriBuilder(builder.HostEnvironment.BaseAddress);

                    if (string.Equals(baseAddressBuilder.Host, "valhalla-wow.com", StringComparison.OrdinalIgnoreCase))
                    {
                        baseAddressBuilder.Host = "www.valhalla-wow.com";
                    }

                    client.BaseAddress = baseAddressBuilder.Uri;
                })
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddHttpClient(WowheadClient.HttpClientKey);

            builder.Services.AddHttpClient(AzureClient.HttpClientKey);

            builder.Services.AddMemoryCache()
                .AddTransient<LocalStorageService>()
                .AddScoped<ApiClient>()
                .AddScoped<WowheadClient>()
                .AddScoped<AzureClient>()
                .AddScoped<WowheadInterop>()
                .AddScoped<ItemProvider>()
                .AddScoped<AzureContainerProvider>()
                .AddSingleton<ItemCache>()
                .AddSingleton<AzureContainerCache>()
                .AddSingleton<TeamsSource>()
                .AddScoped<PermissionManager>()
                .AddScoped<UserTimeProvider>()
                .AddScoped<IAuthorizationHandler, Authorization.CharacterOwnerPolicyHandler>()
                .AddScoped<IAuthorizationHandler, Authorization.TeamLeaderPolicyHandler>()
                .AddScoped<IAuthorizationHandler, Authorization.AdminPolicyHandler>()
                .AddScoped<IAuthorizationHandler, Authorization.MemberPolicyHandler>();

            builder.Services.Configure<JsonSerializerOptions>(options => Serialization.SerializerOptions.ConfigureDefaultOptions(options));

            builder.Services
                .AddApiAuthorization(options =>
                {
                    options.UserOptions.NameClaim = DiscordClaimTypes.Username;
                    options.UserOptions.RoleClaim = AppClaimTypes.Role;
                })
                .AddAccountClaimsPrincipalFactory<RolesClaimsPrincipalFactory>();

            builder.Services.AddAuthorizationCore(AppPolicies.ConfigureAuthorization);

            builder.Services.AddMudServices(options =>
            {
                options.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
                options.SnackbarConfiguration.ShowCloseIcon = false;
                options.SnackbarConfiguration.MaxDisplayedSnackbars = 1;
            });

            var host = builder.Build();

            await host.RunAsync();
        }
    }
}
