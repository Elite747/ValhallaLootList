// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.EntityFrameworkCore;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.SeedAndMigrate;

internal class App : IHostedService
{
    private readonly ApplicationDbContext _context;
    private readonly Seeder.SeederStep _seeder;
    private readonly ItemDeterminer.ItemDeterminerStep _itemDeterminer;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    public App(ApplicationDbContext context, Seeder.SeederStep seeder, ItemDeterminer.ItemDeterminerStep itemDeterminer, IHostApplicationLifetime hostApplicationLifetime)
    {
        _context = context;
        _seeder = seeder;
        _itemDeterminer = itemDeterminer;
        _hostApplicationLifetime = hostApplicationLifetime;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _context.Database.MigrateAsync(cancellationToken);
        await _seeder.SeedAsync(cancellationToken);
        await _itemDeterminer.DetermineAllItemsAsync(cancellationToken);
        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
