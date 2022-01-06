// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Helpers;

public static class NameHelpers
{
    public const string CharacterNameRegex = "(?i)^(?!.*(.)\\1\\1)[a-zA-Z\\u00C0-\\u00D6\\u00D8-\\u00DF\\u00E0-\\u00F6\\u00F8-\\u00FE\\u0100-\\u012F\\u1E9E]{2,12}$";
    public const string GuildNameRegex = "(?i)^((?!.*(.)\\2\\2)[a-zA-Z\\u00C0-\\u00D6\\u00D8-\\u00DF\\u00E0-\\u00F6\\u00F8-\\u00FE\\u0100-\\u012F\\u1E9E]|(?!\\s\\s|\\s$|^\\s)[\\u0020]{1}){2,16}$";

    public static string NormalizeName(string input)
    {
        if (input.Length == 0)
        {
            return input;
        }

        Span<char> span = stackalloc char[input.Length];

        span[0] = char.ToUpperInvariant(input[0]);

        for (int i = 1; i < span.Length; i++)
        {
            span[i] = char.ToLowerInvariant(input[i]);
        }

        if (span.SequenceEqual(input))
        {
            return input;
        }

        return new string(span);
    }
}
