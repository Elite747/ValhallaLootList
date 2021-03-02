// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Pages.Characters
{
    public class LootListSubmissionBracket
    {
        public LootListSubmissionBracket(BracketDto template)
        {
            Template = template;
            Items = new();

            for (int i = template.MaxRank; i >= template.MinRank; i--)
            {
                Items[i] = new uint[template.MaxItems];
            }
        }

        public BracketDto Template { get; }

        public Dictionary<int, uint[]> Items { get; }
    }
}
