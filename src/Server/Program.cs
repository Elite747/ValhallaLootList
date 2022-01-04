// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using ValhallaLootList;
using ValhallaLootList.Client.Shared;
using ValhallaLootList.Serialization;
using ValhallaLootList.Server;
using ValhallaLootList.Server.Authorization;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(_ => TimeZoneInfo.FindSystemTimeZoneById(builder.Configuration.GetValue<string>("RealmTimeZone")));
builder.Services.AddScoped<IAuthorizationHandler, CharacterOwnerPolicyHandler>();
builder.Services.AddScoped<IAuthorizationHandler, TeamLeaderPolicyHandler>();
builder.Services.AddScoped<IAuthorizationHandler, AdminPolicyHandler>();
builder.Services.AddScoped<IAuthorizationHandler, MemberPolicyHandler>();

builder.Services.Configure<DiscordServiceOptions>(options => builder.Configuration.Bind("Discord", options));
builder.Services.AddSingleton<DiscordClientProvider>();
builder.Services.AddHostedService<DiscordBackgroundService>();

builder.Services.AddIdGen(options =>
{
    options.GeneratorId = builder.Configuration.GetValue<int?>("GeneratorId").GetValueOrDefault();
    options.SequenceOverflowStrategy = IdGen.SequenceOverflowStrategy.SpinWait;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<AppUser>(options => options.ClaimsIdentity.RoleClaimType = AppClaimTypes.Role)
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.RemoveAll<IUserValidator<AppUser>>(); // Don't validate usernames or emails. All of this info comes from Discord and doesn't need validation.

builder.Services.AddCors(options => options.AddDefaultPolicy(builder => builder.WithOrigins("https://valhalla-wow.com", "https://www.valhalla-wow.com")));

builder.Services.AddIdentityServer(options => options.IssuerUri = builder.Configuration["IdentityServer:IssuerUri"])
    .AddApiAuthorization<AppUser, ApplicationDbContext>()
    .AddProfileService<IdentityProfileService>();

builder.Services.AddAuthentication()
    .AddDiscord(options =>
    {
        builder.Configuration.Bind("Discord", options);
        options.Scope.Add("identify");
        options.ClaimActions.MapJsonKey(DiscordClaimTypes.Username, "username");
    })
    .AddIdentityServerJwt();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove(AppClaimTypes.Role);

builder.Services.AddAuthorization(AppPolicies.ConfigureAuthorization);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages().AddJsonOptions(options => SerializerOptions.ConfigureDefaultOptions(options.JsonSerializerOptions));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/account/login";
    options.LogoutPath = "/account/logout";
    options.AccessDeniedPath = "/accessdenied";
});
builder.Services.AddApplicationInsightsTelemetry();

builder.Services.AddSingleton(_ => new RenderLocation { IsServer = true });
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseMigrationsEndPoint();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider()
    {
        Mappings =
        {
            [".scss"] = "text/x-scss"
        }
    }
});

app.UseRouting();
app.UseCors();
app.UseIdentityServer();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
    endpoints.MapFallbackToPage("/_Host");
});

app.Run();
