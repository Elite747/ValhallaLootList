// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class RaidStarter
    {
        private readonly RaidSubmissionDto _model = new();

        protected override void OnParametersSet()
        {
            if (Dialog is null) throw new ArgumentNullException(nameof(Dialog));
            if (Team is null) throw new ArgumentNullException(nameof(Team));

            _model.TeamId = Team.Id;
            foreach (var character in Team.Roster)
            {
                _model.Attendees.Add(character.Id);
            }
        }

        private void ToggleAttendee(string id)
        {
            if (_model.Attendees.Contains(id))
            {
                _model.Attendees.Remove(id);
            }
            else
            {
                _model.Attendees.Add(id);
            }
        }

        private Task SubmitAsync()
        {
            return Api.Raids.Create(_model)
                .OnSuccess(raid => Nav.NavigateTo("/raids/" + raid.Id))
                .ValidateWith(_problemValidator)
                .ExecuteAsync();
        }
    }
}
