// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly long _guildId,
            _adminRoleId, _raidLeaderRoleId, _lootMasterRoleId, _recruiterRoleId, _memberRoleId,
            _publicNotificationChannelId;
        private readonly bool _suppressOutgoingMessages;
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
            _adminRoleId = options.Value.AdminRoleId;
            _raidLeaderRoleId = options.Value.RaidLeaderRoleId;
            _lootMasterRoleId = options.Value.LootMasterRoleId;
            _recruiterRoleId = options.Value.RecruiterRoleId;
            _memberRoleId = options.Value.MemberRoleId;
            _publicNotificationChannelId = options.Value.PublicNotificationChannelId;
            _suppressOutgoingMessages = options.Value.SuppressOutgoingMessages;
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
            foreach (var role in member.Roles)
            {
                long roleId = (long)role.Id;

                if (roleId == _adminRoleId)
                {
                    yield return AppRoles.Administrator;
                }

                if (roleId == _memberRoleId)
                {
                    yield return AppRoles.Member;
                }

                if (roleId == _raidLeaderRoleId)
                {
                    yield return AppRoles.RaidLeader;
                }

                if (roleId == _lootMasterRoleId)
                {
                    yield return AppRoles.LootMaster;
                }

                if (roleId == _recruiterRoleId)
                {
                    yield return AppRoles.Recruiter;
                }
            }
        }

        public bool HasAppRole(DiscordMember member, string role)
        {
            if (string.Equals(role, AppRoles.Administrator, StringComparison.OrdinalIgnoreCase))
            {
                return HasAdminRole(member);
            }

            if (string.Equals(role, AppRoles.LootMaster, StringComparison.OrdinalIgnoreCase))
            {
                return HasLootMasterRole(member);
            }

            if (string.Equals(role, AppRoles.RaidLeader, StringComparison.OrdinalIgnoreCase))
            {
                return HasRaidLeaderRole(member);
            }

            if (string.Equals(role, AppRoles.Recruiter, StringComparison.OrdinalIgnoreCase))
            {
                return HasRecruiterRole(member);
            }

            if (string.Equals(role, AppRoles.Member, StringComparison.OrdinalIgnoreCase))
            {
                return HasMemberRole(member);
            }

            return false;
        }

        public bool HasAdminRole(DiscordMember member) => HasDiscordRole(member, _adminRoleId);

        public bool HasMemberRole(DiscordMember member) => HasDiscordRole(member, _memberRoleId);

        public bool HasLootMasterRole(DiscordMember member) => HasDiscordRole(member, _lootMasterRoleId);

        public bool HasRaidLeaderRole(DiscordMember member) => HasDiscordRole(member, _raidLeaderRoleId);

        public bool HasRecruiterRole(DiscordMember member) => HasDiscordRole(member, _recruiterRoleId);

        public bool HasAnyLeadershipRole(DiscordMember member)
        {
            ReadOnlySpan<long> leadershipIds = stackalloc[] { _raidLeaderRoleId, _lootMasterRoleId, _recruiterRoleId };

            foreach (var role in member.Roles)
            {
                if (leadershipIds.IndexOf((long)role.Id) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasDiscordRole(DiscordMember member, string role)
        {
            return member.Roles.Any(r => string.Equals(r.Name, role, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasDiscordRole(DiscordMember member, long roleId)
        {
            var uroleId = (ulong)roleId;
            return member.Roles.Any(r => r.Id == uroleId);
        }

        public async Task SendDmAsync(long id, Action<DiscordMessageBuilder> configureMessage)
        {
            CheckStarted();
            if (_suppressOutgoingMessages)
            {
                return;
            }

            var member = await GetMemberAsync(id);
            var message = new DiscordMessageBuilder();
            configureMessage(message);
            await member.SendMessageAsync(message);
        }

        public async Task<DiscordMessage?> SendOrUpdatePublicNotificationAsync(long? messageId, Action<DiscordMessageBuilder> configureMessage)
        {
            CheckStarted();
            if (_suppressOutgoingMessages || _publicNotificationChannelId == 0L)
            {
                return null;
            }

            var guild = await GetGuildAsync();
            var channel = guild.GetChannel((ulong)_publicNotificationChannelId);

            var messageBuilder = new DiscordMessageBuilder();
            configureMessage(messageBuilder);

            if (messageId > 0)
            {
                var message = await channel.GetMessageAsync((ulong)messageId.Value);

                if (message is not null)
                {
                    return await message.ModifyAsync(messageBuilder);
                }
            }

            return await channel.SendMessageAsync(messageBuilder);
        }

        public async Task DeletePublicNotificationAsync(long messageId)
        {
            CheckStarted();
            var guild = await GetGuildAsync();
            var channel = guild.GetChannel((ulong)_publicNotificationChannelId);
            var message = await channel.GetMessageAsync((ulong)messageId);
            await message.DeleteAsync();
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
            CheckStarted();
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
            CheckStarted();
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
