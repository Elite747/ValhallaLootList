// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Net;

namespace ValhallaLootList.Client.Data;

public interface IApiClientOperation
{
    ApiOperationStatus Status { get; }

    Task Task { get; }

    event Action StatusChanged;

    ProblemDetails GetProblem();

    void ConfigureSuccess(Action<HttpStatusCode> action);

    void ConfigureFailure(Action<ProblemDetails> action);

    Task ExecuteAsync(CancellationToken cancellationToken = default);

    Task ExecuteOrWaitAsync(CancellationToken cancellationToken = default);

    void SetSuccessTask(Func<HttpStatusCode, CancellationToken, Task> task);
}
