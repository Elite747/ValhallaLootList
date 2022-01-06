// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Data;

public class LocalStorageService
{
    private readonly IJSRuntime _js;

    public LocalStorageService(IJSRuntime js) => _js = js;

    public ValueTask<string?> GetAsync(string key)
    {
        return _js.InvokeAsync<string?>("localStorage.getItem", key);
    }

    public ValueTask SetAsync(string key, string? value)
    {
        return _js.InvokeVoidAsync("localStorage.setItem", key, value!);
    }
}
