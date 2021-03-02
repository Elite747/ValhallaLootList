// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data
{
    public class PhaseConfigProvider
    {
        private readonly ApiClient _apiClient;
        private DateTime _expiration;
        private PhaseConfigDto? _phaseConfig;

        public PhaseConfigProvider(ApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public ValueTask<byte> GetCurrentPhaseAsync(CancellationToken cancellationToken = default)
        {
            return GetValueAsync(config => config.CurrentPhase, cancellationToken);
        }

        public ValueTask<IEnumerable<byte>> GetPhasesAsync(CancellationToken cancellationToken = default)
        {
            return GetValueAsync<IEnumerable<byte>>(config => config.Brackets.Keys.OrderBy(phase => phase), cancellationToken);
        }

        public ValueTask<BracketDto[]> GetBracketsAsync(byte phase, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(config => config.Brackets[phase].ToArray(), cancellationToken);
        }

        private ValueTask<TValue> GetValueAsync<TValue>(Func<PhaseConfigDto, TValue> getter, CancellationToken cancellationToken)
        {
            if (CacheIsValid())
            {
                return new(getter(_phaseConfig));
            }

            return new(FromServerAsync(getter, cancellationToken));

            async Task<TValue> FromServerAsync(Func<PhaseConfigDto, TValue> getter, CancellationToken cancellationToken)
            {
                var config = await DownloadPhaseConfigAsync(cancellationToken);
                return getter(config);
            }
        }

        private async Task<PhaseConfigDto> DownloadPhaseConfigAsync(CancellationToken cancellationToken)
        {
            var config = await _apiClient.GetAsync<PhaseConfigDto>("api/v1/config/phases", cancellationToken);
            Debug.Assert(config is not null);
            _phaseConfig = config;
            _expiration = DateTime.UtcNow.AddDays(1);
            return config;
        }

        [MemberNotNullWhen(true, nameof(_phaseConfig))]
        private bool CacheIsValid()
        {
            return _phaseConfig is not null && _expiration > DateTime.UtcNow;
        }
    }
}
