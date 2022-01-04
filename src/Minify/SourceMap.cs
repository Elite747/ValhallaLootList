// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Minify;

public class SourceMap
{
    [JsonPropertyName("version")]
    public int Version { get; set; }

    [JsonPropertyName("sourceRoot")]
    public string? SourceRoot { get; set; }

    [JsonPropertyName("sources")]
    public string[]? Sources { get; set; }

    [JsonPropertyName("names")]
    public object[]? Names { get; set; }

    [JsonPropertyName("mappings")]
    public string? Mappings { get; set; }

    [JsonPropertyName("file")]
    public string? File { get; set; }
}
