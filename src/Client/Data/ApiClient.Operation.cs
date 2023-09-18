// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data;

public partial class ApiClient
{
    private class Operation<TResult> : IApiClientOperation<TResult>
    {
        private readonly HttpClient _httpClient;
        private readonly HttpRequestMessage _request;
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly IMemoryCache _memoryCache;
        private Task? _task;
        private TResult? _result;
        private ProblemDetails? _problem;
        private Action<ProblemDetails>? _failureAction;
        private Action<HttpStatusCode>? _successAction;
        private Action<TResult>? _typedSuccessAction;
        private Func<HttpStatusCode, CancellationToken, Task>? _successTask;
        private Func<TResult, CancellationToken, Task>? _typedSuccessTask;
        private Func<MemoryCacheEntryOptions>? _createCacheEntryOptions;

        public event Action? StatusChanged;

        public Operation(HttpClient httpClient, IMemoryCache memoryCache, JsonSerializerOptions jsonSerializerOptions, HttpRequestMessage request)
        {
            _httpClient = httpClient;
            _request = request;
            _jsonSerializerOptions = jsonSerializerOptions;
            _memoryCache = memoryCache;
        }

        public ApiOperationStatus Status { get; private set; }

        public Task Task => _task ?? Task.CompletedTask;

        public void EnableCaching(Func<MemoryCacheEntryOptions> createEntryOptions)
        {
            _createCacheEntryOptions = createEntryOptions;
        }

        public void DisableCaching()
        {
            _createCacheEntryOptions = null;
        }

        public void ConfigureFailure(Action<ProblemDetails> action)
        {
            _failureAction += action;
        }

        public void ConfigureSuccess(Action<TResult> action)
        {
            _typedSuccessAction += action;
        }

        public void ConfigureSuccess(Action<HttpStatusCode> action)
        {
            _successAction += action;
        }

        public void SetSuccessTask(Func<HttpStatusCode, CancellationToken, Task> task)
        {
            if (_successTask is not null)
            {
                var last = _successTask;
                _successTask = async (code, ct) =>
                {
                    await last(code, ct);
                    await task(code, ct);
                };
            }

            _successTask = task;
        }

        public void SetSuccessTask(Func<TResult, CancellationToken, Task> task)
        {
            if (_typedSuccessTask is not null)
            {
                var last = _typedSuccessTask;
                _typedSuccessTask = async (result, ct) =>
                {
                    await last(result, ct);
                    await task(result, ct);
                };
            }

            _typedSuccessTask = task;
        }

        private ValueTask InvokeHeaderSuccessAsync(HttpStatusCode statusCode, CancellationToken cancellationToken)
        {
            _successAction?.Invoke(statusCode);

            if (_successTask is not null)
            {
                return new(_successTask.Invoke(statusCode, cancellationToken));
            }

            return default;
        }

        private ValueTask InvokeSuccessAsync(TResult result, CancellationToken cancellationToken)
        {
            _typedSuccessAction?.Invoke(result);

            if (_typedSuccessTask is not null)
            {
                return new(_typedSuccessTask.Invoke(result, cancellationToken));
            }

            return default;
        }

        public Task ExecuteAsync(CancellationToken cancellationToken = default)
        {
            if (_task is not null)
            {
                throw new InvalidOperationException("Can't execute an api operation multiple times. Create a new operation instead.");
            }

            return _task = ExecuteInternalAsync(cancellationToken);
        }

        public Task ExecuteOrWaitAsync(CancellationToken cancellationToken = default)
        {
            return _task ??= ExecuteInternalAsync(cancellationToken);
        }

        private async Task ExecuteInternalAsync(CancellationToken cancellationToken = default)
        {
            Status = ApiOperationStatus.Running;
            StatusChanged?.Invoke();
            HttpResponseMessage? response = null;
            bool succeeded = false;
            try
            {
                var cacheKey = _request.RequestUri?.OriginalString;

                if (_createCacheEntryOptions is not null &&
                    cacheKey is not null &&
                    _memoryCache.TryGetValue(cacheKey, out TResult? result) &&
                    result is not null)
                {
                    _result = result;
                    await InvokeHeaderSuccessAsync(HttpStatusCode.OK, cancellationToken);
                    await InvokeSuccessAsync(result, cancellationToken);
                    succeeded = true;
                    return;
                }

                response = await _httpClient.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await InvokeHeaderSuccessAsync(response.StatusCode, cancellationToken);

                    if (response.Content.Headers.ContentLength > 0 || typeof(TResult) != typeof(object))
                    {
                        result = await response.Content.ReadFromJsonAsync<TResult>(_jsonSerializerOptions, cancellationToken);

                        if (result is null)
                        {
                            throw new Exception($"No valid result of type {typeof(TResult).Name} could be read from the response.");
                        }

                        _result = result;

                        if (_createCacheEntryOptions is not null && cacheKey is not null)
                        {
                            _memoryCache.Set(cacheKey, result, _createCacheEntryOptions.Invoke());
                        }

                        await InvokeSuccessAsync(result, cancellationToken);
                    }

                    succeeded = true;
                }
                else if (_failureAction is not null)
                {
                    ProblemDetails? problem = null;
                    try
                    {
                        problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(_jsonSerializerOptions, cancellationToken);
                    }
                    catch { }

                    if (problem is null)
                    {
                        problem = new()
                        {
                            Status = (int)response.StatusCode,
                            Title = response.ReasonPhrase
                        };
                    }
                    else
                    {
                        if (problem.Status.HasValue)
                        {
                            if (problem.Status.Value != (int)response.StatusCode)
                            {
                                throw new Exception("Problem details status code does not match the response status code.");
                            }
                        }
                        else
                        {
                            problem.Status = (int)response.StatusCode;
                        }
                    }

                    _problem = problem;
                    _failureAction.Invoke(problem);
                }
            }
            catch (AccessTokenNotAvailableException exception)
            {
                exception.Redirect();
            }
            catch (OperationCanceledException)
            {
                _problem = new() { Detail = "The operation was canceled." };
            }
            catch (Exception ex)
            {
                _problem = new() { Detail = ex.Message };
                throw;
            }
            finally
            {
                _request.Dispose();
                response?.Dispose();
                Status = succeeded ? ApiOperationStatus.Success : ApiOperationStatus.Failure;
                StatusChanged?.Invoke();
            }
        }

        public bool HasResult()
        {
            return Status == ApiOperationStatus.Success && _result is not null;
        }

        public TResult GetResult()
        {
            if (Status != ApiOperationStatus.Success)
            {
                throw new InvalidOperationException("Can't get the result of an unsuccessful operation.");
            }

            if (_result is null)
            {
                throw new InvalidOperationException("Operation did not have a return value.");
            }

            return _result;
        }

        public ProblemDetails GetProblem()
        {
            if (Status != ApiOperationStatus.Failure)
            {
                throw new InvalidOperationException("Can't get a problem when the operation isn't in a failed state.");
            }

            return GetProblemInternal();
        }

        private ProblemDetails GetProblemInternal()
        {
            return _problem ?? new() { Detail = "An unknown error has occurred." };
        }
    }
}
