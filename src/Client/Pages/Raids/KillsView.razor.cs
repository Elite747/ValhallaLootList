// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using ValhallaLootList.Client.Data.Instances;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class KillsView
    {
        private AddKillInputModel? _addKillInputModel;
        private List<ItemPrioDto>? _assignPrios;
        private EncounterDropDto? _assignDrop;
        private string? _assignEncounterId;

        protected override void OnParametersSet()
        {
            if (Raid is null) throw new ArgumentNullException(nameof(Raid));
        }

        private async Task DeleteAsync(string encounterId)
        {
            try
            {
                var response = await Api.DeleteAsync($"api/v1/raids/{Raid.Id}/Kills/{encounterId}");

                if (response.IsSuccessStatusCode)
                {
                    Raid.Kills.RemoveAll(kill => kill.EncounterId == encounterId);
                    StateHasChanged();
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
        }

        private async Task AddClickedAsync()
        {
            await Instances.EnsureLoadedAsync();
            _addKillInputModel = new(Instances, Raid);
            _addKillModal?.Show();
        }

        private async Task OnSubmitAddKillAsync()
        {
            _addKillModal?.Hide();
            Debug.Assert(_addKillInputModel is not null);
            var dto = new KillSubmissionDto
            {
                Characters = _addKillInputModel.Attendees.Where(pair => pair.Value.Checked).Select(pair => pair.Key.Id!).ToList(),
                Drops = _addKillInputModel.Drops.Where(pair => pair.Value.Checked).Select(pair => pair.Key).ToList(),
                EncounterId = _addKillInputModel.EncounterId
            };

            try
            {
                var response = await Api.PostAsync($"api/v1/raids/{Raid.Id}/kills", dto);

                if (response.IsSuccessStatusCode)
                {
                    var killDto = await response.Content.ReadFromJsonAsync<EncounterKillDto>(Api.JsonSerializerOptions);

                    if (killDto is not null)
                    {
                        Raid.Kills.Add(killDto);
                        StateHasChanged();
                    }
                }
                else
                {
                    // TODO: need some way to handle a failed request.
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
        }

        private async Task BeginAssignAsync(EncounterDropDto drop, string encounterId)
        {
            _assignPrios = null;
            _assignDrop = drop;
            _assignEncounterId = encounterId;
            _assignModal?.Show();
            try
            {
                _assignPrios = await Api.GetAsync<List<ItemPrioDto>>($"api/v1/raids/{Raid.Id}/kills/{encounterId}/drops/{drop.ItemId}/ranks");
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
        }

        private async Task AssignAsync(EncounterDropDto drop, string encounterId, string? characterId)
        {
            _assignModal?.Hide();
            try
            {
                var response = await Api.PutAsync($"api/v1/raids/{Raid.Id}/kills/{encounterId}/drops/{drop.ItemId}", new AwardDropSubmissionDto { WinnerId = characterId });

                if (response.IsSuccessStatusCode)
                {
                    var responseDto = await response.Content.ReadFromJsonAsync<EncounterDropDto>(Api.JsonSerializerOptions);

                    if (responseDto is not null)
                    {
                        drop.AwardedAt = responseDto.AwardedAt;
                        drop.AwardedBy = responseDto.AwardedBy;
                        drop.WinnerId = responseDto.WinnerId;
                        drop.WinnerName = responseDto.WinnerName;
                        StateHasChanged();
                    }
                }
                else
                {
                    // TODO: need some way to handle a failed request.
                }
            }
            catch (AccessTokenNotAvailableException ex)
            {
                ex.Redirect();
            }
        }

        public class AddKillInputModel
        {
            private readonly InstanceProvider _instances;
            private string? _encounterId;

            public AddKillInputModel(InstanceProvider instances, RaidDto raid)
            {
                _instances = instances;

                var lastKill = raid.Kills.Count == 0 ? null : raid.Kills[^1];
                foreach (var character in raid.Attendees)
                {
                    Debug.Assert(character.Id?.Length > 0);
                    Attendees[character] = new(lastKill?.Characters.Contains(character.Id) != false);
                }
            }

            [Required]
            public string? EncounterId
            {
                get => _encounterId;
                set
                {
                    _encounterId = value;
                    Drops.Clear();

                    if (value is not null)
                    {
                        var encounter = _instances.GetCached().SelectMany(i => i.Encounters).FirstOrDefault(e => e.Id == value);

                        if (encounter is not null)
                        {
                            foreach (var drop in encounter.Items)
                            {
                                Drops.Add(drop, new(false));
                            }
                        }
                    }
                }
            }

            public Dictionary<uint, BooleanInput> Drops { get; } = new();

            public Dictionary<CharacterDto, BooleanInput> Attendees { get; } = new();
        }

        public class BooleanInput
        {
            public BooleanInput(bool @checked) => Checked = @checked;

            public bool Checked { get; set; }
        }
    }
}
