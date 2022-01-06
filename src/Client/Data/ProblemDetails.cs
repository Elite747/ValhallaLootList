// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Client.Data;

public class ProblemDetails
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> Extensions { get; } = new Dictionary<string, object>();

    [JsonPropertyName("errors")]
    public Dictionary<string, List<string>>? Errors { get; set; }

    public string GetDisplayString()
    {
        if (Errors?.Count == 1 && Errors.Values.Single() is { Count: 1 } singleErrorList)
        {
            return singleErrorList[0];
        }
        if (Detail?.Length > 0)
        {
            return Detail;
        }
        else if (Title?.Length > 0)
        {
            return Title;
        }
        else if (Status.HasValue)
        {
            return "The server responded with Status Code " + Status.Value;
        }
        else
        {
            return "An unknown error has occurred.";
        }
    }
}
