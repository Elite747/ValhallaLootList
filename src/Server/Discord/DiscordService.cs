﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Server.Discord
{
    public class DiscordService
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _memoryCache;
        private readonly DiscordServiceOptions _options;
        private const string _memberCacheKeyFormat = "_discord_members_{0}", _rolesCacheKey = "_discord_roles";

        public DiscordService(HttpClient client, IOptions<DiscordServiceOptions> options, IMemoryCache memoryCache)
        {
            _options = options.Value;

            if (string.IsNullOrEmpty(options.Value.BotToken))
            {
                throw new Exception("No bot token was specified. Unable to create an instance of DiscordService.");
            }
            client.DefaultRequestHeaders.Authorization = new("Bot", options.Value.BotToken);

            string baseAddress = "https://discord.com/api/";
            if (options.Value.ApiVersion.HasValue)
            {
                baseAddress += 'v' + options.Value.ApiVersion.Value.ToString() + '/';
            }
            client.BaseAddress = new Uri(baseAddress);

            client.DefaultRequestHeaders.AcceptEncoding.Add(new("gzip"));
            client.DefaultRequestHeaders.AcceptEncoding.Add(new("deflate"));
            client.DefaultRequestHeaders.UserAgent.Add(new("VllDiscordBot", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new("application/json"));

            _client = client;
            _memoryCache = memoryCache;
        }

        public async Task<GuildMemberDto?> GetGuildMemberDtoAsync(long? id)
        {
            if (id.HasValue)
            {
                var guildMember = await GetMemberAsync(id.Value);

                if (guildMember?.User is not null)
                {
                    return new GuildMemberDto
                    {
                        Discriminator = guildMember.User.Discriminator,
                        Id = guildMember.User.Id,
                        Nickname = guildMember.Nickname,
                        Username = guildMember.User.Username,
                        Avatar = guildMember.User.Avatar
                    };
                }
            }

            return null;
        }

        public async Task<GuildMemberInfo?> GetMemberInfoAsync(long memberId, CancellationToken cancellationToken = default)
        {
            var member = await GetMemberAsync(memberId, cancellationToken);
            if (member is null) return null;

            var guildRoles = await GetRolesAsync(cancellationToken);
            if (guildRoles is null) throw new Exception("Couldn't load guild roles.");

            var memberRoles = new HashSet<string>();

            foreach (var roleId in member.Roles)
            {
                if (guildRoles.TryGetValue(roleId, out var role))
                {
                    memberRoles.Add(role.Name);
                }
                else
                {
                    _memoryCache.Remove(_rolesCacheKey);
                    guildRoles = await GetRolesAsync(cancellationToken);
                    if (guildRoles is null) throw new Exception("Couldn't load guild roles.");
                    if (guildRoles.TryGetValue(roleId, out role))
                    {
                        memberRoles.Add(role.Name);
                    }
                }
            }

            return new GuildMemberInfo(member, memberRoles);
        }

        public ValueTask<GuildMember?> GetMemberAsync(long memberId, CancellationToken cancellationToken = default)
        {
            return GetAsync<GuildMember>(
                path: $"guilds/{_options.GuildId}/members/{memberId}",
                cacheKey: string.Format(_memberCacheKeyFormat, memberId),
                expiration: TimeSpan.FromMinutes(5),
                cancellationToken: cancellationToken);
        }

        public ValueTask<Dictionary<long, GuildRole>?> GetRolesAsync(CancellationToken cancellationToken = default)
        {
            return GetAsync<Dictionary<long, GuildRole>, List<GuildRole>>(
                path: $"guilds/{_options.GuildId}/roles",
                cacheKey: _rolesCacheKey,
                expiration: TimeSpan.FromDays(1),
                convert: list => list.ToDictionary(role => role.Id),
                cancellationToken: cancellationToken);
        }

        private ValueTask<TValue?> GetAsync<TValue>(string path, string cacheKey, TimeSpan expiration, CancellationToken cancellationToken) where TValue : class
        {
            return GetAsync<TValue, TValue>(path, cacheKey, expiration, x => x, cancellationToken);
        }

        private ValueTask<TValue?> GetAsync<TValue, TDto>(string path, string cacheKey, TimeSpan expiration, Func<TDto, TValue> convert, CancellationToken cancellationToken) where TValue : class
        {
            if (_memoryCache.TryGetValue(cacheKey, out TValue value))
            {
                return new(value);
            }

            return new(GetAndCacheAsync(_client.GetFromJsonAsync<TDto>(path, cancellationToken), convert, cacheKey, expiration));
        }

        private async Task<TValue?> GetAndCacheAsync<TValue, TDto>(Task<TDto?> fromServerTask, Func<TDto, TValue> convert, string cacheKey, TimeSpan expiration) where TValue : class
        {
            var result = await fromServerTask;
            var value = result is null ? null : convert(result);

            if (value is not null)
            {
                _memoryCache.Set(cacheKey, value, expiration);
            }

            return value;
        }
    }
}
