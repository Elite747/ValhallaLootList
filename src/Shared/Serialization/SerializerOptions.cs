// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using System.Text.Json.Serialization;

namespace ValhallaLootList.Serialization
{
    public static class SerializerOptions
    {
        private static JsonSerializerOptions? _options;

        public static JsonSerializerOptions DefaultOptions
        {
            get
            {
                if (_options is null)
                {
                    _options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    ConfigureDefaultOptions(_options);
                }
                return _options;
            }
        }

        public static void ConfigureDefaultOptions(JsonSerializerOptions options)
        {
            options.PropertyNameCaseInsensitive = true;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
            options.Converters.Add(new TimeSpanConverter());
        }
    }
}
