// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public partial class AttendeesView
    {
        protected override void OnParametersSet()
        {
            if (Raid is null) throw new ArgumentNullException(nameof(Raid));
        }

        private Task OnToggled(bool open)
        {
            if (open && _charactersDropdownExecutor is not null)
            {
                return _charactersDropdownExecutor.StartAsync().AsTask();
            }
            return Task.CompletedTask;
        }

        private Task OnAddClickedAsync(CharacterDto character)
        {
            return Api.Raids.AddAttendee(Raid.Id, character.Id)
                .OnSuccess(_ =>
                {
                    Raid.Attendees.Add(character);
                    StateHasChanged();
                })
                .ExecuteAsync();
        }

        private Task OnRemoveClickedAsync(CharacterDto character)
        {
            return Api.Raids.RemoveAttendee(Raid.Id, character.Id)
                .OnSuccess(_ =>
                {
                    Raid.Attendees.RemoveAll(c => c.Id == character.Id);
                    StateHasChanged();
                })
                .ExecuteAsync();
        }
    }
}
