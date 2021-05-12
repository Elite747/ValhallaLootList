// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams.Standings
{
    public class StandingContext
    {
        public StandingContext(LootListDto lootList, LootListEntryDto entry)
        {
            CharacterName = lootList.CharacterName;
            Entry = entry;

            if (Entry.Rank != 0)
            {
                Prio = Entry.Rank;

                Bonuses = lootList.Bonuses.Concat(entry.Bonuses);
                foreach (var bonus in Bonuses)
                {
                    Prio = Prio.Value + bonus.Value;
                }
            }
            else
            {
                Bonuses = Array.Empty<PriorityBonusDto>();
            }
        }

        public string CharacterName { get; }

        public LootListEntryDto Entry { get; }

        public int? Prio { get; set; }

        public IEnumerable<PriorityBonusDto> Bonuses { get; }
    }
}
