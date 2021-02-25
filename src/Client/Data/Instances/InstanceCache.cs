// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Data.Instances
{
    public class InstanceCache : Cache<InstanceDto, string>
    {
        protected override string GetKey(InstanceDto item) => item.Id;
    }
}
