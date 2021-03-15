// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class Create
    {
        private readonly TeamSubmissionDto _team;
        private readonly EditContext _editContext;

        public Create()
        {
            _team = new();
            _editContext = new(_team);
        }

        private void AddScheduleClicked()
        {
            _team.Schedules.Add(new());
        }

        private void RemoveScheduleClicked(int index)
        {
            _team.Schedules.RemoveAt(index);
        }

        private Task OnSubmit()
        {
            if (_editContext.Validate())
            {
                return Api.Teams.Create(_team)
                    .OnSuccess(team => Nav.NavigateTo("/teams/" + team.Name))
                    .ValidateWith(_problemValidator)
                    .ExecuteAsync();
            }

            return Task.CompletedTask;
        }
    }
}
