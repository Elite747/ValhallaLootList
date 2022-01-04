// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Globalization;

namespace ValhallaLootList.Helpers;

public static class DayOfWeekHelpers
{
    public static IEnumerable<DayOfWeek> EnumerateDaysByCulture(CultureInfo culture)
    {
        var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;

        for (DayOfWeek day = firstDayOfWeek; day <= DayOfWeek.Saturday; day++)
        {
            yield return day;
        }
        for (DayOfWeek day = firstDayOfWeek; day < firstDayOfWeek; day++)
        {
            yield return day;
        }
    }
}
