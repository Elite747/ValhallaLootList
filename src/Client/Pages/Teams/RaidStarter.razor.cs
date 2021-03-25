// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ValhallaLootList.Client.Data;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public partial class RaidStarter
    {
        private readonly RaidSubmissionModel _model = new();

        protected override void OnParametersSet()
        {
            if (Dialog is null) throw new ArgumentNullException(nameof(Dialog));
            if (Team is null) throw new ArgumentNullException(nameof(Team));

            foreach (var character in Team.Roster)
            {
                _model.Attendees.Add(character.Id);
            }
        }

        private void ToggleAttendee(long id)
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
            var dto = new RaidSubmissionDto
            {
                Attendees = _model.Attendees.ToList(),
                Phase = _model.Phase,
                TeamId = Team.Id
            };
            return Api.Raids.Create(dto)
                .OnSuccess(raid => Nav.NavigateTo("/raids/" + raid.Id))
                .ValidateWith(_problemValidator)
                .ExecuteAsync();
        }

        public class RaidSubmissionModel
        {
            private HashSet<long>? _attendees;

            [Required]
            public int Phase { get; set; }

            [Required]
            public HashSet<long> Attendees
            {
                get => _attendees ??= new();
                set => _attendees = value;
            }
        }
    }
}
