﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public class LootListSubmissionBracket
    {
        public LootListSubmissionBracket(BracketTemplate template)
        {
            Template = template;
            Items = new();

            for (int i = template.HighestRank; i >= template.LowestRank; i--)
            {
                Items[i] = new uint[template.ItemsPerRow];
            }
        }

        public BracketTemplate Template { get; }

        public Dictionary<int, uint[]> Items { get; }
    }
}
