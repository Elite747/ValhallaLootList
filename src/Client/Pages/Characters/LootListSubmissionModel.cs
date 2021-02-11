// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ValhallaLootList.Client.Pages.Characters
{
    public class LootListSubmissionModel
    {
        [Required]
        public Specializations? MainSpec { get; set; }

        public Specializations? OffSpec { get; set; }

        public List<LootListSubmissionBracket> Brackets { get; } = new();
    }
}
