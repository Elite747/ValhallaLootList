// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(_ => TimeZoneInfo.FindSystemTimeZoneById(Configuration.GetValue<string>("RealmTimeZone")));
            services.AddScoped<PrioCalculator>();
            services.Configure<DiscordServiceOptions>(options => Configuration.Bind("Discord", options));
            services.AddScoped<DiscordRoleMap>();
            services.AddScoped<IAuthorizationHandler, Authorization.CharacterOwnerPolicyHandler>();
            services.AddScoped<IAuthorizationHandler, Authorization.TeamLeaderPolicyHandler>();

            services.AddIdGen(options =>
            {
                options.GeneratorId = Configuration.GetValue<int?>("GeneratorId").GetValueOrDefault();
                options.SequenceOverflowStrategy = IdGen.SequenceOverflowStrategy.SpinWait;
            });

            services.AddHttpClient<DiscordService>()
                .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

            services.AddDatabaseDeveloperPageExceptionFilter();

            services
                .AddDefaultIdentity<AppUser>(options =>
                {
                    options.ClaimsIdentity.RoleClaimType = AppClaimTypes.Role;
                    options.ClaimsIdentity.UserNameClaimType = AppClaimTypes.Name;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.RemoveAll<IUserValidator<AppUser>>(); // Don't validate usernames or emails. All of this info comes from Discord and doesn't need validation.

            services.AddIdentityServer()
                .AddApiAuthorization<AppUser, ApplicationDbContext>(options =>
                {
                    var claims = options.IdentityResources["openid"].UserClaims;
                    claims.Add(AppClaimTypes.Role);
                    claims.Add(AppClaimTypes.Name);
                    claims.Add(AppClaimTypes.RaidLeader);
                    claims.Add(AppClaimTypes.Character);
                    claims = options.ApiResources.Single().UserClaims;
                    claims.Add(AppClaimTypes.Role);
                    claims.Add(AppClaimTypes.Name);
                    claims.Add(AppClaimTypes.RaidLeader);
                    claims.Add(AppClaimTypes.Character);
                })
                .AddProfileService<IdentityProfileService>();

            services.AddAuthentication()
                .AddDiscord(options =>
                {
                    Configuration.Bind("Discord", options);
                    options.Scope.Add("identify");
                    options.ClaimActions.MapJsonKey(DiscordClaimTypes.Username, "username");
                })
                .AddIdentityServerJwt();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Remove(AppClaimTypes.Role);

            services.AddAuthorization(AppPolicies.ConfigureAuthorization);

            services.AddControllersWithViews();
            services.AddRazorPages().AddJsonOptions(options => Serialization.SerializerOptions.ConfigureDefaultOptions(options.JsonSerializerOptions));

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/account/login";
                options.LogoutPath = "/account/logout";
                options.AccessDeniedPath = "/accessdenied";
            });
            services.AddApplicationInsightsTelemetry();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
