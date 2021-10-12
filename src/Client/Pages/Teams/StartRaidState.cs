// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Teams
{
    public class StartRaidState
    {
        public Dictionary<long, CharacterDto> Attendees { get; } = new();

        public Dictionary<long, CharacterDto> Rto { get; } = new();
    }
}
