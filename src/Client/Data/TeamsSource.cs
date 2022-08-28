// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data;

public class TeamsSource
{
    private bool _initialized;
    private List<TeamNameDto>? _teams;

    public event Action? Updated;

    public IEnumerable<TeamNameDto> GetTeams(int? size = null, bool includeInactive = false)
    {
        if (_teams is null)
        {
            return Array.Empty<TeamNameDto>();
        }

        if (includeInactive)
        {
            if (size is null)
            {
                return _teams;
            }
            return _teams.Where(team => team.Size == size);
        }

        if (size is null)
        {
            return _teams.Where(team => !team.Inactive);
        }
        return _teams.Where(team => !team.Inactive && team.Size == size);
    }

    public TeamNameDto? GetById(long id)
    {
        return _teams?.Find(team => team.Id == id);
    }

    public Task RefreshAsync(ApiClient api)
    {
        _initialized = true;
        return api.Teams.GetAllTeamNames()
            .OnSuccess(teams =>
            {
                _teams = teams;
                Updated?.Invoke();
            })
            .ExecuteAsync();
    }

    public void EnsureStarted(ApiClient api)
    {
        if (!_initialized)
        {
            _ = RefreshAsync(api);
        }
    }

    public ValueTask EnsureStartedAsync(ApiClient api)
    {
        if (_initialized)
        {
            return default;
        }

        return new(RefreshAsync(api));
    }
}
