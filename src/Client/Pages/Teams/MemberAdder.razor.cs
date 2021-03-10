// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class MemberAdder
    {
        private readonly InputModel _input = new();

        protected override void OnParametersSet()
        {
            if (Team is null) throw new ArgumentNullException(nameof(Team));
        }

        private Task OnSubmit()
        {
            Debug.Assert(Team.Id?.Length > 0);
            Debug.Assert(_input.Id?.Length > 0);
            return Api.Teams.AddMember(Team.Id, _input.Id)
                .OnSuccess(_ =>
                {
                    _modal?.Hide();
                    MemberAdded.InvokeAsync();
                })
                .ValidateWith(_serverValidator)
                .ExecuteAsync();
        }

        private class InputModel
        {
            [System.ComponentModel.DataAnnotations.Required]
            public string? Id { get; set; }
        }
    }
}
