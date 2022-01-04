// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Client.Data;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.Client.Shared;

public class UserTimeProvider
{
    private readonly LocalStorageService _localStorage;
    private readonly TimeZoneInfo _serverTimeZone;
    private const string _storageKey = "timemode";

    public UserTimeProvider(LocalStorageService localStorage)
    {
        _localStorage = localStorage;
        _serverTimeZone = TimeZoneInfo.FromSerializedString("Eastern Standard Time;-300;(UTC-05:00) Eastern Time (US & Canada);Eastern Standard Time;Eastern Daylight Time;[01:01:0001;12:31:2006;60;[0;02:00:00;4;1;0;];[0;02:00:00;10;5;0;];][01:01:2007;12:31:9999;60;[0;02:00:00;3;2;0;];[0;02:00:00;11;1;0;];];");
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

    public DateTimeOffset Now => Mode switch
    {
        TimeFormatterMode.Original => DateTimeOffset.UtcNow.ToTimeZone(_serverTimeZone),
        TimeFormatterMode.Local => DateTimeOffset.Now,
        TimeFormatterMode.Utc => DateTimeOffset.UtcNow,
        _ => DateTimeOffset.Now
    };

    public DateTime Today => Now.Date;

    public DateTimeOffset FromServerTimeOfDay(DateTime date, TimeSpan timeOfDay)
    {
        var dateTime = date.Date.Add(timeOfDay);
        return Convert(new DateTimeOffset(dateTime, _serverTimeZone.GetUtcOffset(dateTime)));
    }

    public DateTimeOffset Convert(DateTimeOffset date)
    {
        return Mode switch
        {
            TimeFormatterMode.Original => date.ToTimeZone(_serverTimeZone),
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
