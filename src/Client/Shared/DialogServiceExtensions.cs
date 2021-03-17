// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace ValhallaLootList.Client.Shared
{
    public static class DialogServiceExtensions
    {
        public static async Task<TResult?> ShowAsync<TComponent, TResult>(
            this IDialogService dialogService,
            string title,
            DialogParameters? parameters = null,
            DialogOptions? options = null)
            where TComponent : ComponentBase
        {
            var dialog = dialogService.Show<TComponent>(title, parameters, options);

            var result = await dialog.Result;

            if (result.Cancelled)
            {
                return default;
            }

            return (TResult)result.Data;
        }
    }
}