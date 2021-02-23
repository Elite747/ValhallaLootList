// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using ValhallaLootList.Helpers;

namespace ValhallaLootList.Server.Data
{
    public static class MySqlTranslations
    {
        public static DateTime ConvertTz(DateTime dateTime, string fromTz, string toTz)
        {
            return new DateTimeOffset(dateTime, TimeSpanHelpers.ParseTimeZoneOffsetString(fromTz)).ToOffset(TimeSpanHelpers.ParseTimeZoneOffsetString(toTz)).DateTime;
        }
    }
}
