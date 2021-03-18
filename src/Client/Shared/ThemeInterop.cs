// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Shared
{
    public sealed class ThemeInterop : ComponentBase, IDisposable
    {
        private readonly ThemeInteropHelper _helper;
        private readonly DotNetObjectReference<ThemeInteropHelper> _helperRef;
        private bool _started;

        public ThemeInterop()
        {
            _helper = new(this);
            _helperRef = DotNetObjectReference.Create(_helper);
        }

        [Inject] public IJSRuntime JS { get; set; } = null!;

        [Parameter] public EventCallback<bool> ThemeChanged { get; set; }

        protected override async Task OnInitializedAsync()
        {
            if (!_started)
            {
                _started = true;
                await JS.InvokeVoidAsync("valhallaLootList.addThemeListener", _helperRef);
            }
        }

        private Task InvokeChange(bool isDark)
        {
            return ThemeChanged.InvokeAsync(isDark);
        }

        private class ThemeInteropHelper
        {
            private readonly ThemeInterop _parent;

            public ThemeInteropHelper(ThemeInterop parent)
            {
                _parent = parent;
            }

            [JSInvokable("SetIsDark")]
            public Task SetIsDarkAsync(bool isDark)
            {
                return _parent.InvokeChange(isDark);
            }
        }

        public void Dispose()
        {
            _helperRef.Dispose();
        }
    }
}