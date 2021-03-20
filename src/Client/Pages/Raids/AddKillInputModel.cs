// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Raids
{
    public class AddKillInputModel
    {
        private string? _encounterId;

        public AddKillInputModel(IEnumerable<InstanceDto> instances, RaidDto raid)
        {
            Instances = instances.Where(i => i.Phase == raid.Phase).OrderBy(i => i.Name);
            Raid = raid;

            var lastKill = raid.Kills.Count == 0 ? null : raid.Kills[^1];
            foreach (var id in raid.Attendees.Select(a => a.Character?.Id))
            {
                if (id?.Length > 0 && lastKill?.Characters.Contains(id) != false)
                {
                    Attendees.Add(id);
                }
            }
        }

        public RaidDto Raid { get; set; }

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
                            Drops.Add(drop, 0);
                        }
                    }
                }
            }
        }

        public Dictionary<uint, int> Drops { get; } = new();

        public HashSet<string> Attendees { get; } = new();

        public void ToggleAttendee(string id)
        {
            if (!Attendees.Add(id))
            {
                Attendees.Remove(id);
            }
        }
    }
}
