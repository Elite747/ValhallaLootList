// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data;

public abstract class Cache<TItem, TKey> where TItem : class where TKey : notnull
{
    private readonly IMemoryCache _memoryCache;

    protected Cache(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    protected abstract TKey GetKey(TItem item);

    protected virtual MemoryCacheEntryOptions CreateCacheEntryOptions(TItem item)
    {
        return new MemoryCacheEntryOptions().RegisterPostEvictionCallback(OnEvictedInternal);
    }

    protected virtual void OnEvicted(TKey key, TItem value, EvictionReason reason, object state)
    {
    }

    private void OnEvictedInternal(object key, object value, EvictionReason reason, object state)
    {
        OnEvicted((TKey)key, (TItem)value, reason, state);
    }

    public void Update(TItem item)
    {
        _memoryCache.Set(GetKey(item), item, CreateCacheEntryOptions(item));
    }

    public bool TryGet(TKey key, out TItem item)
    {
        return _memoryCache.TryGetValue(key, out item);
    }

    public TItem GetOrAdd(TKey key, Func<TKey, TItem> getter)
    {
        if (_memoryCache.TryGetValue(key, out TItem item))
        {
            return item;
        }

        item = getter(key);

        _memoryCache.Set(key, item, CreateCacheEntryOptions(item));

        return item;
    }

    public ValueTask<TItem> GetOrAddAsync(TKey key, Func<TKey, Task<TItem>> getter)
    {
        if (_memoryCache.TryGetValue(key, out TItem item))
        {
            return new(item);
        }

        return new(AddAndReturnAsync(getter(key)));
    }

    public ValueTask<TItem> GetOrAddAsync(TKey key, Func<TKey, CancellationToken, Task<TItem>> getter, CancellationToken cancellationToken)
    {
        if (_memoryCache.TryGetValue(key, out TItem item))
        {
            return new(item);
        }

        return new(AddAndReturnAsync(getter(key, cancellationToken)));
    }

    private async Task<TItem> AddAndReturnAsync(Task<TItem> getter)
    {
        var item = await getter;

        _memoryCache.Set(GetKey(item), item, CreateCacheEntryOptions(item));

        return item;
    }
}
