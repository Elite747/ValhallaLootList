// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.SeedAndMigrate;
using ValhallaLootList.SeedAndMigrate.ItemDeterminer;
using ValhallaLootList.SeedAndMigrate.Seeder;
using ValhallaLootList.Server.Data;

IHost host = Host.CreateDefaultBuilder(args)
    .UseConsoleLifetime()
    .ConfigureHostConfiguration(config => config.AddUserSecrets(typeof(Program).Assembly))
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<App>();

        services.AddScoped<SeederStep>();
        services.AddScoped<ItemDeterminerStep>();

        services.Configure<Config>(config =>
        {
            config.SeedInstancesPath = context.Configuration.GetValue<string>(nameof(config.SeedInstancesPath));
            config.SeedItemsPath = context.Configuration.GetValue<string>(nameof(config.SeedItemsPath));
        });

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(context.Configuration.GetConnectionString("DefaultConnection"), sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
    })
    .Build();

await host.RunAsync();
