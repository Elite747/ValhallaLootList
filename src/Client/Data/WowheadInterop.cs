﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Data
{
    public class WowheadInterop
    {
        private readonly IJSRuntime _js;

        public WowheadInterop(IJSRuntime js)
        {
            _js = js;
        }

        public ValueTask HideTooltipAsync(CancellationToken cancellationToken = default)
        {
            return _js.InvokeVoidAsync("WH.Tooltip.hide", cancellationToken);
        }
    }
}
