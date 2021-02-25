// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ValhallaLootList.Client.Data
{
    public abstract class Cache<TItem, TKey> where TItem : class where TKey : notnull
    {
        protected abstract TKey GetKey(TItem item);

        private readonly ConcurrentDictionary<TKey, TItem> _cache = new();

        public bool IsEmpty => _cache.IsEmpty;

        public IEnumerable<TItem> EnumerateCached() => _cache.Values;

        public TItem? GetByKey(TKey key) => _cache.TryGetValue(key, out var item) ? item : null;

        public void UpdateCache(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                UpdateCache(item);
            }
        }

        public void UpdateCache(TItem item)
        {
            _cache.AddOrUpdate(GetKey(item), item, (_, _) => item);
        }
    }
}
