﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
            foreach (var character in raid.Attendees)
            {
                Debug.Assert(character.Id?.Length > 0);
                Attendees[character] = new(lastKill?.Characters.Contains(character.Id) != false);
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
                            Drops.Add(drop, new(0));
                        }
                    }
                }
            }
        }

        public Dictionary<uint, NumberInput> Drops { get; } = new();

        public Dictionary<CharacterDto, BooleanInput> Attendees { get; } = new();
    }
}
