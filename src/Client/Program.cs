// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using MudBlazor.Services;
using ValhallaLootList;
using ValhallaLootList.Client;
using ValhallaLootList.Client.Authorization;
using ValhallaLootList.Client.Data;
using ValhallaLootList.Client.Data.Containers;
using ValhallaLootList.Client.Data.Items;
using ValhallaLootList.Client.Shared;
using ValhallaLootList.Serialization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

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
    .AddScoped<IAuthorizationHandler, CharacterOwnerPolicyHandler>()
    .AddScoped<IAuthorizationHandler, TeamLeaderPolicyHandler>()
    .AddScoped<IAuthorizationHandler, AdminPolicyHandler>()
    .AddScoped<IAuthorizationHandler, MemberPolicyHandler>();

builder.Services.Configure<JsonSerializerOptions>(options => SerializerOptions.ConfigureDefaultOptions(options));

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

builder.Services.AddSingleton(_ => new RenderLocation { IsServer = false });
builder.Services.AddSingleton<IThemeProvider, ThemeProvider>();

var host = builder.Build();

await host.RunAsync();