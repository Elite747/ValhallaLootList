// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Options;
using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Shared
{
    public class TitleManager
    {
        private readonly IJSInProcessRuntime _js;
        private readonly TitleManagerOptions _options;
        private string? _currentTitle, _originalTitle;

        public TitleManager(IJSRuntime js, IOptions<TitleManagerOptions> options)
        {
            _js = (IJSInProcessRuntime)js;
            _options = options.Value;
        }

        public void Update(string? subtitle, bool hidePrimary = false)
        {
            if (_originalTitle is null)
            {
                _originalTitle = _js.Invoke<string>("titleManager.getTitle") ?? string.Empty;
                _currentTitle = _originalTitle;
            }

            subtitle ??= string.Empty;

            string newTitle = hidePrimary ? subtitle : TryJoin(subtitle, _options.PrimaryTitle ?? _originalTitle, _options.Separator ?? " - ");

            if (string.CompareOrdinal(_currentTitle, newTitle) != 0)
            {
                _js.InvokeVoid("titleManager.setTitle", newTitle);
                _currentTitle = newTitle;
            }
        }

        private static string TryJoin(string left, string right, string separator)
        {
            if (left.Length > 0)
            {
                if (right.Length > 0)
                {
                    return string.Concat(left, separator, right);
                }

                return left;
            }
            return right;
        }
    }
}
