// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data;

public interface IApiClientOperation<out TResult> : IApiClientOperation
{
    bool HasResult();

    TResult GetResult();

    void ConfigureSuccess(Action<TResult> action);

    void SetSuccessTask(Func<TResult, CancellationToken, Task> task);

    void EnableCaching(Func<MemoryCacheEntryOptions> createEntryOptions);

    void DisableCaching();
}
