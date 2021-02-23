// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client
{
    public class InstanceCache
    {
        private readonly ConcurrentDictionary<string, InstanceDto> _cache = new();

        public bool IsEmpty => _cache.IsEmpty;

        public IEnumerable<InstanceDto> EnumerateCached() => _cache.Values;

        public InstanceDto? GetById(string id) => _cache.TryGetValue(id, out var instance) ? instance : null;

        public void UpdateCache(IEnumerable<InstanceDto> instances)
        {
            foreach (var instance in instances)
            {
                if (instance.Id is null)
                {
                    throw new ArgumentException("Instance ID cannot be null.");
                }

                _cache.AddOrUpdate(instance.Id, instance, (_, _) => instance);
            }
        }
    }
}
