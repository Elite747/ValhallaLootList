// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Discord
{
    public sealed class DiscordClientProvider : IDisposable
    {
        private readonly DiscordClient _client;
        private readonly long _guildId;
        private readonly Dictionary<string, string> _appRoleMap;
        private bool _started, _disposed;

        public DiscordClientProvider(IOptions<DiscordServiceOptions> options, ILoggerFactory loggerFactory)
        {
            _client = new DiscordClient(new()
            {
                Token = options.Value.BotToken,
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory,
                Intents = DiscordIntents.GuildMembers | DiscordIntents.Guilds | DiscordIntents.GuildPresences
            });
            _guildId = options.Value.GuildId;
            _appRoleMap = options.Value.AppRoleMap;
        }

        public DiscordClient Client
        {
            get
            {
                CheckStarted();
                return _client;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _started = false;
                _client.Dispose();
            }
        }

        public ValueTask EnsureStartedAsync()
        {
            CheckDisposed();
            if (_started) return default;

            _started = true;
            return new(_client.ConnectAsync());
        }

        public async Task<DiscordGuild> GetGuildAsync()
        {
            await EnsureStartedAsync();
            return await _client.GetGuildAsync((ulong)_guildId);
        }

        public async Task<DiscordMember> GetMemberAsync(long id)
        {
            var guild = await GetGuildAsync();
            return await guild.GetMemberAsync((ulong)id);
        }

        public IEnumerable<string> GetAppRoles(DiscordMember member)
        {
            foreach (var (appRole, discordRole) in _appRoleMap)
            {
                if (IsInDiscordRole(member, discordRole))
                {
                    yield return appRole;
                }
            }
        }

        public bool IsInAppRole(DiscordMember member, string role)
        {
            if (_appRoleMap.TryGetValue(role, out var discordRole))
            {
                return IsInDiscordRole(member, discordRole);
            }

            return false;
        }

        public bool IsInDiscordRole(DiscordMember member, string role)
        {
            return member.Roles.Any(r => string.Equals(r.Name, role, StringComparison.OrdinalIgnoreCase));
        }

        public async Task SendDmAsync(long id, Action<DiscordMessageBuilder> configureMessage)
        {
            var member = await GetMemberAsync(id);
            var message = new DiscordMessageBuilder();
            configureMessage(message);
            await member.SendMessageAsync(message);
        }

        public async Task SendAsync(long channelId, Action<DiscordMessageBuilder> configureMessage)
        {
            var guild = await GetGuildAsync();
            var channel = guild.GetChannel((ulong)channelId);
            await channel.SendMessageAsync(configureMessage);
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(null, "The discord client has been disposed.");
        }

        private void CheckStarted()
        {
            CheckDisposed();
            if (!_started)
            {
                throw new InvalidOperationException("The discord client has not been started yet.");
            }
        }

        public GuildMemberDto CreateDto(DiscordMember member)
        {
            return new GuildMemberDto
            {
                AppRoles = GetAppRoles(member).ToList(),
                Avatar = member.AvatarHash,
                DiscordRoles = member.Roles.Select(r => r.Name).ToList(),
                Discriminator = member.Discriminator,
                Id = (long)member.Id,
                Nickname = member.Nickname,
                Username = member.Username
            };
        }

        public async Task<GuildMemberDto?> GetMemberDtoAsync(long? id)
        {
            if (id > 0)
            {
                var member = await GetMemberAsync(id.Value);

                if (member is not null)
                {
                    return CreateDto(member);
                }
            }

            return null;
        }
    }
}
