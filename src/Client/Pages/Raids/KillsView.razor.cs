// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class KillsView
    {
        private AddKillInputModel? _addKillInputModel;
        private IList<ItemPrioDto>? _assignPrios;
        private EncounterDropDto? _assignDrop;

        protected override void OnParametersSet()
        {
            if (Raid is null) throw new ArgumentNullException(nameof(Raid));
        }

        private async Task DeleteAsync(string encounterId)
        {
            Debug.Assert(Raid.Id?.Length > 0);
            await Api.Raids.Delete(Raid.Id, encounterId)
                .OnSuccess(_ =>
                {
                    Raid.Kills.RemoveAll(kill => kill.EncounterId == encounterId);
                    StateHasChanged();
                })
                .ExecuteAsync();
        }

        private async Task AddClickedAsync()
        {
            await Api.Instances.GetAll()
                .OnSuccess(instances =>
                {
                    _addKillInputModel = new(instances, Raid);
                    _addKillModal?.Show();
                })
                .ExecuteAsync();
        }

        private async Task OnSubmitAddKillAsync()
        {
            _addKillModal?.Hide();
            Debug.Assert(_addKillInputModel is not null);
            Debug.Assert(Raid.Id?.Length > 0);

            var kill = new KillSubmissionDto
            {
                Characters = _addKillInputModel.Attendees.Where(pair => pair.Value.Checked).Select(pair => pair.Key.Id!).ToList(),
                EncounterId = _addKillInputModel.EncounterId
            };

            foreach (var (id, count) in _addKillInputModel.Drops)
            {
                for (int i = 0; i < count.Value; i++)
                {
                    kill.Drops.Add(id);
                }
            }

            await Api.Raids
                .AddKill(Raid.Id, kill)
                .OnSuccess(kill =>
                {
                    Raid.Kills.Add(kill);
                    StateHasChanged();
                })
                .ExecuteAsync();
        }

        private Task BeginAssignAsync(EncounterDropDto drop)
        {
            Debug.Assert(Raid.Id?.Length > 0);
            _assignPrios = null;
            _assignDrop = drop;
            _assignModal?.Show();

            return Api.Drops.GetPriorityRankings(drop.Id)
                .OnSuccess(prios => _assignPrios = prios)
                .ExecuteAsync();
        }

        private Task AssignAsync(EncounterDropDto drop, string? characterId)
        {
            Debug.Assert(Raid.Id?.Length > 0);
            _assignModal?.Hide();

            return Api.Drops.Assign(drop.Id, characterId)
                .OnSuccess(response =>
                {
                    drop.AwardedAt = response.AwardedAt;
                    drop.AwardedBy = response.AwardedBy;
                    drop.WinnerId = response.WinnerId;
                    drop.WinnerName = response.WinnerName;
                    StateHasChanged();
                })
                .ExecuteAsync();
        }

        public class AddKillInputModel
        {
            private string? _encounterId;

            public AddKillInputModel(IEnumerable<InstanceDto> instances, RaidDto raid)
            {
                Instances = instances.Where(i => i.Phase == raid.Phase).OrderBy(i => i.Name);

                var lastKill = raid.Kills.Count == 0 ? null : raid.Kills[^1];
                foreach (var character in raid.Attendees)
                {
                    Debug.Assert(character.Id?.Length > 0);
                    Attendees[character] = new(lastKill?.Characters.Contains(character.Id) != false);
                }
            }

            public IEnumerable<InstanceDto> Instances { get; }

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
                        var encounter = Instances.SelectMany(i => i.Encounters).FirstOrDefault(e => e.Id == value);

                        if (encounter is not null)
                        {
                            foreach (var drop in encounter.Items)
                            {
                                Drops.Add(drop, new(0));
                            }
                        }
                    }
                }
            }

            public Dictionary<uint, NumberInput> Drops { get; } = new();

            public Dictionary<CharacterDto, BooleanInput> Attendees { get; } = new();
        }

        public class BooleanInput
        {
            public BooleanInput(bool @checked) => Checked = @checked;

            public bool Checked { get; set; }
        }

        public class NumberInput
        {
            public NumberInput(int value) => Value = value;

            [Range(0, 5)]
            public int Value { get; set; }
        }
    }
}
