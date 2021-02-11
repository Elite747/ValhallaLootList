﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public class ItemSelectionContext
    {
        public IEnumerable<ItemDto>? Items { get; set; }

        public LootListSubmissionBracket? Bracket { get; set; }

        public int Rank { get; set; }

        public int Column { get; set; }
    }
}
