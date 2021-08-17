// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Shared
{
    public sealed class ThemeProvider : IThemeProvider
    {
        private readonly IJSInProcessRuntime _js;
        private Theme? _selectedTheme;
        private bool _initialized;

        public ThemeProvider(IJSRuntime js)
        {
            _js = (IJSInProcessRuntime)js;
        }

        public Theme? SelectedTheme
        {
            get
            {
                if (!_initialized)
                {
                    _initialized = true;

                    var theme = _js.Invoke<string>("valhallaLootList.getTheme");

                    if (string.Equals(theme, "light", StringComparison.OrdinalIgnoreCase))
                    {
                        return _selectedTheme = Theme.Light;
                    }
                    else if (string.Equals(theme, "dark", StringComparison.OrdinalIgnoreCase))
                    {
                        return _selectedTheme = Theme.Dark;
                    }
                    else
                    {
                        return _selectedTheme = null;
                    }
                }
                return _selectedTheme;
            }
            set
            {
                if (_selectedTheme != value)
                {
                    _selectedTheme = value;
                    _js.InvokeVoid("valhallaLootList.setTheme", value switch
                    {
                        Theme.Light => "light",
                        Theme.Dark => "dark",
                        _ => null
                    });
                }
            }
        }
    }
}