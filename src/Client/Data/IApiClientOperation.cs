// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ValhallaLootList.Client.Data
{
    public interface IApiClientOperation
    {
        void ConfigureSuccess(Action<HttpStatusCode> action);

        void ConfigureFailure(Action<ProblemDetails> action);

        Task ExecuteAsync(CancellationToken cancellationToken = default);

        void SetSuccessTask(Func<HttpStatusCode, CancellationToken, Task> task);
    }
}
