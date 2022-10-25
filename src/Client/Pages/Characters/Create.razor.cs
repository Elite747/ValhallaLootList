// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using MudBlazor;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters;

public partial class Create
{
    private Task OnSubmit()
    {
        return (EditingCharacter is null ? Api.Characters.Create(_character) : Api.Characters.Update(EditingCharacter.Id, _character))
            .OnSuccess((CharacterDto character, CancellationToken ct) =>
            {
                Dialog.Close(DialogResult.Ok(character));
                if (_character.SenderIsOwner)
                {
                    return Permissions.RefreshAsync(ct);
                }
                return Task.CompletedTask;
            })
            .ValidateWith(_problemValidator)
            .ExecuteAsync();
    }

    private void RaceChanged(PlayerRace? newValue)
    {
        _character.Race = newValue;
        _raceClasses = newValue?.TryGetClasses(out var raceClasses) == true ? raceClasses : ClassesExtensions.GetAll();

        if (!_raceClasses.Contains(_character.Class))
        {
            _character.Class = default;
        }
    }
}
