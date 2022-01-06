// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using ValhallaLootList.Client.Data;
using ValhallaLootList.Client.Shared;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids;

public partial class KillsView
{
    protected override void OnParametersSet()
    {
        if (Raid is null) throw new ArgumentNullException(nameof(Raid));
    }

    private async Task DeleteAsync(string encounterId, byte trashIndex)
    {
        await Api.Raids.Delete(Raid.Id, encounterId, trashIndex)
            .OnSuccess(_ =>
            {
                Raid.Kills.RemoveAll(kill => kill.EncounterId == encounterId && kill.TrashIndex == trashIndex);
                StateHasChanged();
            })
            .SendErrorTo(Snackbar)
            .ExecuteAsync();
    }

    private Task AddClickedAsync()
    {
        return DialogService.ShowAsync<AddKillWizard, object?>(
            string.Empty,
            parameters: new()
            {
                [nameof(AddKillWizard.Raid)] = Raid
            });
    }

    private string GetWinnerName(long? id)
    {
        if (id.HasValue)
        {
            return Raid.Attendees.Find(a => a.Character.Id == id)?.Character.Name ?? "Unknown";
        }
        return "nobody";
    }

    private IEnumerable<string> GetIneligible(EncounterKillDto kill)
    {
        foreach (var attendee in Raid.Attendees)
        {
            if (!kill.Characters.Contains(attendee.Character.Id))
            {
                yield return attendee.Character.Name;
            }
        }
    }

    private async Task BeginAssignAsync(EncounterDropDto drop)
    {
        var characterId = await DialogService.ShowAsync<AssignLootDialog, long?>(
            string.Empty,
            parameters: new()
            {
                [nameof(AssignLootDialog.Drop)] = drop,
                [nameof(AssignLootDialog.Raid)] = Raid
            });

        if (characterId.HasValue)
        {
            await AssignAsync(drop, characterId);
        }
    }

    private Task AssignAsync(EncounterDropDto drop, long? characterId)
    {
        return Api.Drops.Assign(drop.Id, characterId)
            .OnSuccess(response =>
            {
                drop.AwardedAt = response.AwardedAt;
                drop.AwardedBy = response.AwardedBy;
                drop.WinnerId = response.WinnerId;
                StateHasChanged();
            })
            .SendErrorTo(Snackbar)
            .ExecuteAsync();
    }

    private Task DeleteRaidAsync()
    {
        return Api.Raids.Delete(Raid.Id)
            .OnSuccess(_ => Nav.NavigateTo("teams/" + Raid.TeamName))
            .SendErrorTo(Snackbar)
            .ExecuteAsync();
    }
}
