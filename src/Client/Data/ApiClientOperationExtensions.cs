// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using MudBlazor;
using ValhallaLootList.Client.Shared;

namespace ValhallaLootList.Client.Data
{
    public static class ApiClientOperationExtensions
    {
        public static IApiClientOperation<TResult> CacheFor<TResult>(this IApiClientOperation<TResult> operation, TimeSpan cacheTime)
        {
            operation.EnableCaching(() => new MemoryCacheEntryOptions().SetAbsoluteExpiration(cacheTime));
            return operation;
        }

        public static async Task<TResult?> ExecuteAndTryReturnAsync<TResult>(this IApiClientOperation<TResult> operation, CancellationToken cancellationToken = default)
            where TResult : class
        {
            TResult? result = null;

            await operation.OnSuccess(r => result = r).ExecuteAsync(cancellationToken);

            return result;
        }

        public static async Task<TResult> ExecuteAndEnsureReturnAsync<TResult>(this IApiClientOperation<TResult> operation, CancellationToken cancellationToken = default)
            where TResult : class
        {
            TResult? result = null;

            await operation
                .OnSuccess(r => result = r)
                .OnFailure(problem => throw new HttpRequestException(problem.Detail, null, (HttpStatusCode?)problem.Status))
                .ExecuteAsync(cancellationToken);

            Debug.Assert(result is not null);

            return result;
        }

        public static IApiClientOperation OnSuccess(this IApiClientOperation operation, Action<HttpStatusCode> action)
        {
            operation.ConfigureSuccess(action);
            return operation;
        }

        public static IApiClientOperation OnSuccess(this IApiClientOperation operation, Func<HttpStatusCode, CancellationToken, Task> task)
        {
            operation.SetSuccessTask(task);
            return operation;
        }

        public static IApiClientOperation<TResult> OnSuccess<TResult>(this IApiClientOperation<TResult> operation, Action<TResult> action)
        {
            operation.ConfigureSuccess(action);
            return operation;
        }

        public static IApiClientOperation<TResult> OnSuccess<TResult>(this IApiClientOperation<TResult> operation, Func<TResult, CancellationToken, Task> task)
        {
            operation.SetSuccessTask(task);
            return operation;
        }

        public static IApiClientOperation OnFailure(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(action);
            return operation;
        }

        public static IApiClientOperation OnFailureStatusCode(this IApiClientOperation operation, HttpStatusCode statusCode, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status == (int)statusCode)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnClientFault(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is >= 400 and < 500)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnBadRequest(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is 400)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnUnauthorized(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is 401 or 403)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnNotFound(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is 404)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnServerFault(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is >= 500 and < 600)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation OnInternalServerError(this IApiClientOperation operation, Action<ProblemDetails> action)
        {
            operation.ConfigureFailure(problem =>
            {
                if (problem.Status is 500)
                {
                    action(problem);
                }
            });
            return operation;
        }

        public static IApiClientOperation ValidateWith(this IApiClientOperation operation, CustomValidator? validator)
        {
            if (validator is not null)
            {
                operation.ConfigureFailure(problem =>
                {
                    if (problem.Errors?.Count > 0)
                    {
                        validator.DisplayErrors(problem.Errors);
                    }
                });
            }
            return operation;
        }

        public static IApiClientOperation ValidateWith(this IApiClientOperation operation, ProblemValidator? validator)
        {
            if (validator is not null)
            {
                operation.ConfigureFailure(validator.DisplayErrors);
            }
            return operation;
        }

        public static IApiClientOperation SendErrorTo(this IApiClientOperation operation, ISnackbar snackbar)
        {
            operation.ConfigureFailure(problem => snackbar.Add(problem.GetDisplayString(), Severity.Error));
            return operation;
        }
    }
}
