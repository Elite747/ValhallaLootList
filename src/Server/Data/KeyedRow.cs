// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;

namespace ValhallaLootList.Server.Data
{
    public class KeyedRow
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
    }
}
