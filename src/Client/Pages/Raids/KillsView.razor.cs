// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.Client.Shared;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class KillsView
    {
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
                .SendErrorTo(Snackbar)
                .ExecuteAsync();
        }

        private Task AddClickedAsync()
        {
            return Api.Instances.GetAll()
                .OnSuccess(instances => DialogService.Show<AddKillDialog>(
                    "Add Kill",
                    parameters: new() { [nameof(AddKillDialog.Input)] = new AddKillInputModel(instances, Raid) },
                    options: new() { FullWidth = true, MaxWidth = MudBlazor.MaxWidth.Medium }))
                .SendErrorTo(Snackbar)
                .ExecuteAsync();
        }

        private async Task BeginAssignAsync(EncounterDropDto drop)
        {
            var characterId = await DialogService.ShowAsync<AssignLootDialog, string?>(
                "Assigning " + (drop.ItemName ?? "Item"),
                parameters: new()
                {
                    [nameof(AssignLootDialog.Drop)] = drop,
                    [nameof(AssignLootDialog.Raid)] = Raid
                });

            if (characterId?.Length > 0)
            {
                await AssignAsync(drop, characterId);
            }
        }

        private Task AssignAsync(EncounterDropDto drop, string? characterId)
        {
            return Api.Drops.Assign(drop.Id, characterId)
                .OnSuccess(response =>
                {
                    drop.AwardedAt = response.AwardedAt;
                    drop.AwardedBy = response.AwardedBy;
                    drop.WinnerId = response.WinnerId;
                    drop.WinnerName = response.WinnerName;
                    StateHasChanged();
                })
                .SendErrorTo(Snackbar)
                .ExecuteAsync();
        }
    }
}
