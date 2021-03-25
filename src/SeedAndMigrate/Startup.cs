// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate
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
                config.SeedInstancesPath = Configuration.GetValue<string>(nameof(config.SeedInstancesPath));
                config.SeedItemsPath = Configuration.GetValue<string>(nameof(config.SeedItemsPath));
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"), sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
        }
    }
}
