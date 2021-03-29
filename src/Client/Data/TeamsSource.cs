// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class TeamsSource
    {
        private bool _initialized;
        private IList<TeamNameDto>? _teams;

        public event Action? Updated;

        public IEnumerable<TeamNameDto> Teams => _teams ?? Array.Empty<TeamNameDto>();

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
}
