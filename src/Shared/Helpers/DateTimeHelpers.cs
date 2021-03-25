// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Helpers
{
    public static class DateTimeHelpers
    {
        public static DateTimeOffset TimeZoneNow(this TimeZoneInfo timeZoneInfo)
        {
            return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZoneInfo);
        }

        public static DateTimeOffset ToTimeZone(this DateTimeOffset date, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(date, timeZone);
        }
    }
}
