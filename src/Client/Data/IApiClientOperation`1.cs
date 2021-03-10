// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data
{
    public interface IApiClientOperation<TResult> : IApiClientOperation
    {
        void ConfigureSuccess(Action<TResult> action);

        void SetSuccessTask(Func<TResult, CancellationToken, Task> task);

        void EnableCaching(Func<MemoryCacheEntryOptions> createEntryOptions);

        void DisableCaching();
    }
}
