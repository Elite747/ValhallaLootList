// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared
{
    public class UserTimeProvider
    {
        private readonly LocalStorageService _localStorage;
        private const string _storageKey = "timemode";

        public UserTimeProvider(LocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _ = InitializeAsync();
        }

        public TimeFormatterMode Mode { get; private set; }

        public event Action? ModeChanged;

        public async Task SetModeAsync(TimeFormatterMode mode)
        {
            await _localStorage.SetAsync(_storageKey, mode.ToString());
            Mode = mode;
            ModeChanged?.Invoke();
        }

        public DateTimeOffset Convert(DateTimeOffset date)
        {
            return Mode switch
            {
                TimeFormatterMode.Original => date,
                TimeFormatterMode.Local => date.ToLocalTime(),
                TimeFormatterMode.Utc => date.ToUniversalTime(),
                _ => date
            };
        }

        private async Task<TimeFormatterMode> GetModeAsync()
        {
            if (Enum.TryParse<TimeFormatterMode>(await _localStorage.GetAsync(_storageKey), true, out var mode))
            {
                return mode;
            }
            return TimeFormatterMode.Original;
        }

        private async Task InitializeAsync() => Mode = await GetModeAsync();
    }
}
