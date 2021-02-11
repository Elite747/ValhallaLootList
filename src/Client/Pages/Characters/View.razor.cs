// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public partial class View
    {
        private CharacterDto? _character;
        private List<LootListDto>? _lootLists;

        protected override async Task OnParametersSetAsync()
        {
            _character = null;
            _lootLists = null;
            if (!string.IsNullOrWhiteSpace(Character))
            {
                try
                {
                    string characterPath = "api/v1/characters/" + Character;
                    _character = await Api.GetAsync<CharacterDto>(characterPath);

                    if (_character is not null)
                    {
                        _lootLists = await Api.GetAsync<List<LootListDto>>(characterPath + "/LootLists");
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
