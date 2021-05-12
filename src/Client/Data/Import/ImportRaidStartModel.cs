// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;

namespace ValhallaLootList.Client.Data.Import
{
    public class ImportRaidStartModel
    {
        public List<ImportCharacter>? Characters { get; set; }

        public string? InstanceId { get; set; }
    }
}
