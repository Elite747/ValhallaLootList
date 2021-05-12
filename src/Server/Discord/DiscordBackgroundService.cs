// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Discord
{
    public class DiscordBackgroundService : BackgroundService
    {
        private readonly DiscordClientProvider _discordClientProvider;
        private readonly IServiceProvider _serviceProvider;

        public DiscordBackgroundService(DiscordClientProvider discordClientProvider, IServiceProvider serviceProvider)
        {
            _discordClientProvider = discordClientProvider;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _discordClientProvider.EnsureStartedAsync();
            await LoadAllMembersAsync();
            RegisterCallbacks();
            try
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
            UnregisterCallbacks();
        }

        private async Task LoadAllMembersAsync()
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var guild = await _discordClientProvider.GetGuildAsync();

            await foreach (var user in context.Users.AsTracking().AsAsyncEnumerable())
            {
                if (guild.Members.TryGetValue((ulong)user.Id, out var member) && member.DisplayName != user.UserName)
                {
                    user.UserName = member.DisplayName;
                    user.NormalizedUserName = member.DisplayName.Normalize().ToUpper();
                }
            }

            await context.SaveChangesAsync();

            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }

        private void RegisterCallbacks()
        {
            _discordClientProvider.Client.GuildMemberUpdated += OnGuildMemberUpdatedAsync;
        }

        private void UnregisterCallbacks()
        {
            _discordClientProvider.Client.GuildMemberUpdated -= OnGuildMemberUpdatedAsync;
        }

        private async Task OnGuildMemberUpdatedAsync(DiscordClient sender, GuildMemberUpdateEventArgs e)
        {
            using var scope = _serviceProvider.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var user = await context.Users.FindAsync((long)e.Member.Id);

            if (user is not null && user.UserName != e.Member.DisplayName)
            {
                user.UserName = e.Member.DisplayName;
                user.NormalizedUserName = user.UserName.Normalize().ToUpper();
                await context.SaveChangesAsync();
            }

            if (scope is IAsyncDisposable asyncDisposable)
            {
                await asyncDisposable.DisposeAsync();
            }
        }
    }
}
