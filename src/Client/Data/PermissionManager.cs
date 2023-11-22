// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Caching.Memory;
using MudBlazor;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class PermissionManager(ApiClient api, IMemoryCache memoryCache, ISnackbar snackbar)
{
    private readonly ApiClient _api = api;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private readonly ISnackbar _snackbar = snackbar;
    private const string _cacheKey = "_permissions";

    public async Task<IEnumerable<long>> GetOwnedCharacterIdsAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(_cacheKey, out PermissionsDto? permissions))
        {
            return permissions!.Characters;
        }
        else
        {
            await RefreshAsync(cancellationToken);

            if (_memoryCache.TryGetValue(_cacheKey, out permissions))
            {
                return permissions!.Characters;
            }
        }
        return Array.Empty<long>();
    }

    public async Task<bool> IsOwnerOfAsync(long characterId, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(_cacheKey, out PermissionsDto? permissions))
        {
            return permissions!.Characters.Contains(characterId);
        }

        await RefreshAsync(cancellationToken);

        if (_memoryCache.TryGetValue(_cacheKey, out permissions))
        {
            return permissions!.Characters.Contains(characterId);
        }

        return false;
    }

    public async Task<bool> IsLeaderOfAsync(long teamId, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(_cacheKey, out PermissionsDto? permissions))
        {
            return permissions!.Teams.Contains(teamId);
        }

        await RefreshAsync(cancellationToken);

        if (_memoryCache.TryGetValue(_cacheKey, out permissions))
        {
            return permissions!.Teams.Contains(teamId);
        }

        return false;
    }

    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        return _api.Permissions.Get()
            .OnSuccess(permissions => _memoryCache.Set(_cacheKey, permissions, TimeSpan.FromHours(1)))
            .SendErrorTo(_snackbar)
            .ExecuteAsync(cancellationToken);
    }
}
