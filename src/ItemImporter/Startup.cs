// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValhallaLootList.ItemImporter.WarcraftDatabase;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.ItemImporter
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHostedService<App>();

            services.Configure<Config>(config =>
            {
                config.BossNameOverrides = new();
                config.BossOverrides = new();
                config.Instances = new();
                config.Tokens = new();

                foreach (var section in Configuration.GetSection(nameof(config.BossNameOverrides)).GetChildren())
                {
                    config.BossNameOverrides[section.Key] = section.Value;
                }

                foreach (var section in Configuration.GetSection(nameof(config.BossOverrides)).GetChildren())
                {
                    config.BossOverrides[uint.Parse(section.Key)] = section.Value;
                }

                foreach (var section in Configuration.GetSection(nameof(config.Instances)).GetChildren())
                {
                    var instanceConfig = new InstanceConfig();
                    section.Bind(instanceConfig);
                    config.Instances[section.Key] = instanceConfig;//new(); section.Bind( section.GetChildren().Select(child => uint.Parse(child.Value)).ToArray();
                }

                foreach (var section in Configuration.GetSection(nameof(config.Tokens)).GetChildren())
                {
                    config.Tokens[uint.Parse(section.Key)] = section.GetChildren().Select(child => uint.Parse(child.Value)).ToArray();
                }
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), MySqlServerVersion.LatestSupportedServerVersion));

            services.AddDbContext<WowDataContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("WowConnection"), MySqlServerVersion.LatestSupportedServerVersion));
        }
    }
}
