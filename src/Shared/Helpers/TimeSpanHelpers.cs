// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Globalization;

namespace ValhallaLootList.Helpers
{
    public static class TimeSpanHelpers
    {
        public static string GetTimeZoneOffsetString(TimeSpan timeSpan)
        {
            return new DateTimeOffset(default(DateTime), timeSpan).ToString("zzz");
        }

        public static TimeSpan ParseTimeZoneOffsetString(string str)
        {
            return DateTimeOffset.ParseExact("2000-01-01T00:00:00.0000000" + str, "O", CultureInfo.InvariantCulture).Offset;
        }
    }
}
