// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics.CodeAnalysis;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Options;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Discord;

public sealed class DiscordClientProvider : IDisposable
{
    private readonly DiscordClient _client;
    private readonly DiscordServiceOptions _options;
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
        _options = options.Value;
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
        if (_started)
        {
            return default;
        }

        _started = true;
        return new(_client.ConnectAsync());
    }

    public async Task<DiscordGuild> GetGuildAsync()
    {
        await EnsureStartedAsync();
        return await _client.GetGuildAsync((ulong)_options.GuildId);
    }

    public async Task<DiscordMember?> GetMemberAsync(long id)
    {
        var guild = await GetGuildAsync();

        try
        {
            return await guild.GetMemberAsync((ulong)id);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public IEnumerable<string> GetAppRoles(DiscordMember member)
    {
        foreach (var role in member.Roles)
        {
            long roleId = (long)role.Id;

            if (roleId == _options.AdminRoleId)
            {
                yield return AppRoles.Administrator;
            }

            if (roleId == _options.MemberRoleId)
            {
                yield return AppRoles.Member;
            }

            if (roleId == _options.RaidLeaderRoleId)
            {
                yield return AppRoles.RaidLeader;
            }

            if (roleId == _options.LootMasterRoleId)
            {
                yield return AppRoles.LootMaster;
            }

            if (roleId == _options.RecruiterRoleId)
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

    public bool HasAdminRole(DiscordMember member)
    {
        return HasDiscordRole(member, _options.AdminRoleId);
    }

    public bool HasMemberRole(DiscordMember member)
    {
        return HasDiscordRole(member, _options.MemberRoleId);
    }

    public bool HasLootMasterRole(DiscordMember member)
    {
        return HasDiscordRole(member, _options.LootMasterRoleId);
    }

    public bool HasRaidLeaderRole(DiscordMember member)
    {
        return HasDiscordRole(member, _options.RaidLeaderRoleId);
    }

    public bool HasRecruiterRole(DiscordMember member)
    {
        return HasDiscordRole(member, _options.RecruiterRoleId);
    }

    public bool HasAnyLeadershipRole(DiscordMember member)
    {
        ReadOnlySpan<long> leadershipIds = [_options.RaidLeaderRoleId, _options.LootMasterRoleId, _options.RecruiterRoleId, _options.LeadershipRoleId];

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
        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member is not null)
        {
            var message = new DiscordMessageBuilder();
            configureMessage(message);
            await member.SendMessageAsync(message);
        }
    }

    public async Task SendDmAsync(long id, string message)
    {
        CheckStarted();

        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member is not null)
        {
            await member.SendMessageAsync(message);
        }
    }

    public Task AddRoleAsync(long id, string roleName, string reason)
    {
        return AddRoleAsync(id, reason, role => string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase));
    }

    private async Task AddRoleAsync(long id, string reason, Func<DiscordRole, bool> predicate)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member?.Roles.Any(predicate) == false)
        {
            var guild = await GetGuildAsync();
            var role = guild.Roles.Values.FirstOrDefault(predicate);

            if (role is not null)
            {
                await member.GrantRoleAsync(role, reason);
            }
        }
    }

    public async Task RemoveRoleAsync(long id, string roleName, string reason)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member is not null)
        {
            var role = member.Roles.FirstOrDefault(role => string.Equals(role.Name, roleName, StringComparison.OrdinalIgnoreCase));

            if (role is not null)
            {
                await member.RevokeRoleAsync(role, reason);
            }
        }
    }

    public async Task AssignLeadershipRolesAsync(long id, bool raidLeader, bool lootMaster, bool recruiter, string reason)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member is not null)
        {
            var guild = await GetGuildAsync();

            if (CanGrant((ulong)_options.LeadershipRoleId, guild, member, out var role))
            {
                await member.GrantRoleAsync(role, reason);
            }

            if (raidLeader && CanGrant((ulong)_options.RaidLeaderRoleId, guild, member, out role))
            {
                await member.GrantRoleAsync(role, reason);
            }

            if (lootMaster && CanGrant((ulong)_options.LootMasterRoleId, guild, member, out role))
            {
                await member.GrantRoleAsync(role, reason);
            }

            if (recruiter && CanGrant((ulong)_options.RecruiterRoleId, guild, member, out role))
            {
                await member.GrantRoleAsync(role, reason);
            }
        }

        static bool CanGrant(ulong roleId, DiscordGuild guild, DiscordMember member, [NotNullWhen(true)] out DiscordRole? role)
        {
            if (guild.Roles.TryGetValue(roleId, out role))
            {
                return !member.Roles.Any(r => r.Id == roleId);
            }
            return false;
        }
    }

    public async Task RemoveLeadershipRolesAsync(long id, string reason)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages)
        {
            return;
        }

        var member = await GetMemberAsync(id);

        if (member is not null)
        {
            foreach (var role in member.Roles.Where(IsLeadershipRole).ToList())
            {
                await member.RevokeRoleAsync(role, reason);
            }
        }
    }

    private bool IsLeadershipRole(DiscordRole role)
    {
        long roleId = (long)role.Id;
        return roleId == _options.LeadershipRoleId
            || roleId == _options.LootMasterRoleId
            || roleId == _options.RaidLeaderRoleId
            || roleId == _options.RecruiterRoleId;
    }

    public async Task<DiscordMessage?> SendOrUpdateOfficerNotificationAsync(long? messageId, Action<DiscordMessageBuilder> configureMessage)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages || _options.OfficerNotificationChannelId == 0L)
        {
            return null;
        }

        var channel = await GetChannelAsync(_options.OfficerNotificationChannelId);

        if (channel is null)
        {
            return null;
        }

        var messageBuilder = new DiscordMessageBuilder();
        configureMessage(messageBuilder);

        try
        {
            if (messageId > 0)
            {
                var message = await channel.GetMessageAsync((ulong)messageId);
                return await message.ModifyAsync(messageBuilder);
            }

            return await channel.SendMessageAsync(messageBuilder);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task DeleteOfficerNotificationAsync(long messageId)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages || _options.OfficerNotificationChannelId == 0L)
        {
            return;
        }

        var channel = await GetChannelAsync(_options.OfficerNotificationChannelId);

        if (channel is not null)
        {
            try
            {
                var message = await channel.GetMessageAsync((ulong)messageId);
                await message.DeleteAsync();
            }
            catch (NotFoundException)
            {
            }
        }
    }

    public async Task<DiscordMessage?> SendOrUpdatePublicNotificationAsync(long? messageId, Action<DiscordMessageBuilder> configureMessage)
    {
        CheckStarted();
        if (_options.SuppressOutgoingMessages || _options.PublicNotificationChannelId == 0L)
        {
            return null;
        }

        var channel = await GetChannelAsync(_options.PublicNotificationChannelId);

        if (channel is null)
        {
            return null;
        }

        var messageBuilder = new DiscordMessageBuilder();
        configureMessage(messageBuilder);

        try
        {
            if (messageId > 0)
            {
                var message = await channel.GetMessageAsync((ulong)messageId);
                return await message.ModifyAsync(messageBuilder);
            }

            return await channel.SendMessageAsync(messageBuilder);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    public async Task DeletePublicNotificationAsync(long messageId)
    {
        CheckStarted();

        var channel = await GetChannelAsync(_options.PublicNotificationChannelId);

        if (channel is not null)
        {
            try
            {
                var message = await channel.GetMessageAsync((ulong)messageId);
                await message.DeleteAsync();
            }
            catch (NotFoundException)
            {
            }
        }
    }

    private async Task<DiscordChannel?> GetChannelAsync(long channelId)
    {
        var guild = await GetGuildAsync();
        try
        {
            return guild.GetChannel((ulong)channelId);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }

    private void CheckDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(null, "The discord client has been disposed.");
        }
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
