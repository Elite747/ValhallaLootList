// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams.Standings
{
    public partial class Index
    {
        private readonly List<LootListDto> _lootLists = new();
        private List<CharacterDto>? _characters;

        protected override Task OnParametersSetAsync()
        {
            return RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            _characters = null;
            _lootLists.Clear();
            if (!string.IsNullOrWhiteSpace(Team))
            {
                try
                {
                    _characters = await Api.GetAsync<List<CharacterDto>>("api/v1/characters?team=" + Team);
                    StateHasChanged();

                    if (_characters?.Count > 0)
                    {
                        foreach (var character in _characters)
                        {
                            var ll = await Api.GetAsync<List<LootListDto>>($"api/v1/characters/{character.Id}/lootlists");

                            if (ll != null)
                            {
                                _lootLists.AddRange(ll);
                            }
                        }
                    }
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
            }
        }
    }
}
