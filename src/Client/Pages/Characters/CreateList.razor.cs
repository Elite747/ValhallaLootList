// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public partial class CreateList
    {
        private static readonly Dictionary<Classes, (Specializations Spec, string DisplayName)[]> _specLookup = new()
        {
            [Classes.Druid] = new[]
            {
                (Specializations.BalanceDruid, "Balance"),
                (Specializations.BearDruid, "Feral (Tank)"),
                (Specializations.CatDruid, "Feral (DPS)"),
                (Specializations.RestoDruid, "Restoration")
            },
            [Classes.Hunter] = new[] { (Specializations.Hunter, "Beast Mastery / Marksmanship / Survival") },
            [Classes.Mage] = new[] { (Specializations.Mage, "Arcane / Fire / Frost") },
            [Classes.Paladin] = new[]
            {
                (Specializations.HolyPaladin, "Holy"),
                (Specializations.ProtPaladin, "Protection"),
                (Specializations.RetPaladin, "Retribution")
            },
            [Classes.Priest] = new[]
            {
                (Specializations.HealerPriest, "Discipline / Holy"),
                (Specializations.ShadowPriest, "Shadow")
            },
            [Classes.Rogue] = new[] { (Specializations.Rogue, "Assassination / Combat / Subtlety") },
            [Classes.Shaman] = new[]
            {
                (Specializations.EleShaman, "Elemental"),
                (Specializations.EnhanceShaman, "Enhancement"),
                (Specializations.RestoShaman, "Restoration")
            },
            [Classes.Warlock] = new[] { (Specializations.Warlock, "Affliction / Demonology / Destruction") },
            [Classes.Warrior] = new[]
            {
                (Specializations.ArmsWarrior, "Arms"),
                (Specializations.FuryWarrior, "Fury"),
                (Specializations.ProtWarrior, "Protection")
            }
        };

        private readonly LootListSubmissionModel _lootList = new();
        private (Specializations Spec, string DisplayName)[]? _classSpecializations;
        private IList<ItemDto>? _items = Array.Empty<ItemDto>();

        protected override async Task OnInitializedAsync()
        {
            if (Character is null)
            {
                throw new Exception("Character cannot be null.");
            }
            _classSpecializations = _specLookup[Character.Class];

            try
            {
                _lootList.Brackets.Clear();

                var template = await Api.GetAsync<BracketTemplate[]>($"api/v1/characters/lootlisttemplate?phase={Phase}");

                if (template is not null)
                {
                    foreach (var bracketTemplate in template)
                    {
                        _lootList.Brackets.Add(new(bracketTemplate));
                    }
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
        }

        private Task MainSpecChanged(Specializations? spec)
        {
            _lootList.MainSpec = spec;
            return UpdateItemListAsync();
        }

        private Task OffSpecChanged(Specializations? spec)
        {
            _lootList.OffSpec = spec;
            return UpdateItemListAsync();
        }

        private async Task UpdateItemListAsync()
        {
            try
            {
                _items = null;
                StateHasChanged();

                if (_lootList.MainSpec.HasValue)
                {
                    var spec = _lootList.MainSpec.Value;

                    if (_lootList.OffSpec.HasValue)
                    {
                        spec |= _lootList.OffSpec.Value;
                    }

                    _items = (await Api.GetAsync<ItemDto[]>($"api/v1/items?phase={Phase}&spec={spec}")) ?? Array.Empty<ItemDto>();
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
            StateHasChanged();
        }

        private void OnSelectionRequested(ItemSelectionContext context)
        {
            Debug.Assert(_modal is not null);

            var alreadySelected = new HashSet<uint>();

            foreach (var bracket in _lootList.Brackets)
            {
                foreach (var (rank, items) in bracket.Items)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        var itemId = items[i];
                        if (rank != context.Rank || i != context.Column)
                        {
                            alreadySelected.Add(itemId);
                        }
                    }
                }
            }

            var contextItems = _items?.Where(item => !alreadySelected.Contains(item.Id));

            if (context.Bracket?.Template.AllowTypeDuplicates == false)
            {
                var alreadySelectedGroups = new HashSet<ItemGroup>();

                foreach (var (rank, items) in context.Bracket.Items)
                {
                    for (int i = 0; i < items.Length; i++)
                    {
                        var itemId = items[i];
                        if (rank != context.Rank || i != context.Column)
                        {
                            var item = _items?.FirstOrDefault(x => x.Id == itemId);

                            if (item != null)
                            {
                                alreadySelectedGroups.Add(new ItemGroup(item.Type, item.Slot));
                            }
                        }
                    }
                }

                contextItems = contextItems?.Where(item => !alreadySelectedGroups.Contains(new ItemGroup(item.Type, item.Slot)));
            }

            context.Items = contextItems?.ToList();
            _modalContext = context;
            _modal.Show();
        }

        private void OnItemSelected(uint selectedId)
        {
            Debug.Assert(_bracketValidator is not null);
            Debug.Assert(_modalContext?.Bracket is not null);

            _modal?.Hide();

            if (!_lootList.MainSpec.HasValue)
            {
                return;
            }

            _modalContext.Bracket.Items[_modalContext.Rank][_modalContext.Column] = selectedId;

            _bracketValidator.ClearErrors();
            var errors = new Dictionary<string, List<string>>();

            var allItems = new HashSet<uint>();
            var bracketItemGroups = new HashSet<ItemGroup>();

            for (int bracketNumber = 0; bracketNumber < _lootList.Brackets.Count; bracketNumber++)
            {
                var bracket = _lootList.Brackets[bracketNumber];
                var bracketSpec = (bracket.Template.AllowOffSpec && _lootList.OffSpec.HasValue) ? (_lootList.MainSpec.Value | _lootList.OffSpec.Value) : _lootList.MainSpec.Value;

                foreach (var (rank, items) in bracket.Items)
                {
                    for (int col = 0; col < items.Length; col++)
                    {
                        uint id = items[col];
                        if (id > 0)
                        {
                            var item = _items?.FirstOrDefault(i => i.Id == id);
                            if (item is null)
                            {
                                AddError(errors, rank, col, "Item does not exist or is not for your chosen specializations.");
                            }
                            else
                            {
                                // check that there are no duplicate items.
                                if (!allItems.Add(id))
                                {
                                    AddError(errors, rank, col, "Duplicate items are not allowed.");
                                }

                                // check that each bracket follows the unique item type rule.
                                if (!bracket.Template.AllowTypeDuplicates && !bracketItemGroups.Add(new ItemGroup(item.Type, item.Slot)))
                                {
                                    AddError(errors, rank, col, $"Cannot have multiple items of the same type in Bracket {bracketNumber}.");
                                }

                                if (item.Restrictions.Any(r => r.Level == ItemRestrictionLevel.Unequippable && (r.Specs & bracketSpec) != 0))
                                {
                                    AddError(errors, rank, col, "Item is not equippable by your class or specialization.");
                                }
                            }
                        }
                    }
                }

                bracketItemGroups.Clear();
            }

            _bracketValidator.DisplayErrors(errors);

            StateHasChanged();
        }

        private static void AddError(Dictionary<string, List<string>> errors, int rank, int column, string error)
        {
            var key = $"Rank{rank} ({column + 1})";

            if (!errors.TryGetValue(key, out var rankErrors))
            {
                rankErrors = errors[key] = new();
            }

            rankErrors.Add(error);
        }

        private async Task OnSubmit()
        {
            try
            {
                var dto = new LootListSubmissionDto
                {
                    Items = new(),
                    MainSpec = _lootList.MainSpec,
                    OffSpec = _lootList.OffSpec
                };

                foreach (var bracket in _lootList.Brackets)
                {
                    foreach (var (rank, items) in bracket.Items)
                    {
                        dto.Items.Add(rank, items);
                    }
                }

                var response = await Api.PostAsync($"api/v1/characters/{Character.Id}/lootlists/{Phase}", dto);

                if (response.IsSuccessStatusCode)
                {
                    var responseDto = await response.Content.ReadFromJsonAsync<LootListDto>();

                    await OnLootListCreated.InvokeAsync(responseDto);
                }
                else
                {
                    var problemDto = await response.Content.ReadFromJsonAsync<ProblemDetails>();

                    if (problemDto?.Errors != null)
                    {
                        _serverValidator?.DisplayErrors(problemDto.Errors);
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
