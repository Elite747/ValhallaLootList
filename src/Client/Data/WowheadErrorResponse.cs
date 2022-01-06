// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json.Serialization;

namespace ValhallaLootList.Client.Data;

public class WowheadErrorResponse
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}
