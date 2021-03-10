// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace ValhallaLootList.Client.Data
{
    public partial class ApiClient
    {
        private class Operation<TResult> : IApiClientOperation<TResult>
        {
            private readonly HttpClient _httpClient;
            private readonly HttpRequestMessage _request;
            private readonly JsonSerializerOptions _jsonSerializerOptions;
            private readonly IMemoryCache _memoryCache;
            private bool _executed;
            private Action<ProblemDetails>? _failureAction;
            private Action<HttpStatusCode>? _successAction;
            private Action<TResult>? _typedSuccessAction;
            private Func<TResult, CancellationToken, Task>? _typedSuccessTask;
            private Func<MemoryCacheEntryOptions>? _createCacheEntryOptions;

            public Operation(HttpClient httpClient, IMemoryCache memoryCache, JsonSerializerOptions jsonSerializerOptions, HttpRequestMessage request)
            {
                _httpClient = httpClient;
                _request = request;
                _jsonSerializerOptions = jsonSerializerOptions;
                _memoryCache = memoryCache;
            }

            public void EnableCaching(Func<MemoryCacheEntryOptions> createEntryOptions) => _createCacheEntryOptions = createEntryOptions;

            public void DisableCaching() => _createCacheEntryOptions = null;

            public void ConfigureFailure(Action<ProblemDetails> action) => _failureAction += action;

            public void ConfigureSuccess(Action<TResult> action) => _typedSuccessAction += action;

            public void ConfigureSuccess(Action<HttpStatusCode> action) => _successAction += action;

            public void SetSuccessTask(Func<TResult, CancellationToken, Task> task)
            {
                if (_typedSuccessTask is not null)
                {
                    throw new InvalidOperationException("Only one async success callback can be set per operation.");
                }

                _typedSuccessTask = task;
            }

            public async Task ExecuteAsync(CancellationToken cancellationToken = default)
            {
                if (_executed)
                {
                    throw new InvalidOperationException("Can't execute an api operation multiple times. Create a new operation instead.");
                }

                _executed = true;

                if (_createCacheEntryOptions is not null && _memoryCache.TryGetValue(_request.RequestUri, out TResult? result) && result is not null)
                {
                    _successAction?.Invoke(HttpStatusCode.OK);
                    _typedSuccessAction?.Invoke(result);
                    return;
                }

                HttpResponseMessage? response = null;
                try
                {
                    response = await _httpClient.SendAsync(_request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _successAction?.Invoke(response.StatusCode);

                        if (_typedSuccessAction is not null || _typedSuccessTask is not null)
                        {
                            result = await response.Content.ReadFromJsonAsync<TResult>(_jsonSerializerOptions, cancellationToken);

                            if (result is null)
                            {
                                throw new Exception("No valid result of type {typeof(TResult)} could be read from the response.");
                            }

                            if (_createCacheEntryOptions is not null)
                            {
                                _memoryCache.Set(_request.RequestUri, result, _createCacheEntryOptions.Invoke());
                            }

                            _typedSuccessAction?.Invoke(result);

                            if (_typedSuccessTask is not null)
                            {
                                await _typedSuccessTask.Invoke(result, cancellationToken);
                            }
                        }
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

                        _failureAction.Invoke(problem);
                    }
                }
                catch (AccessTokenNotAvailableException exception)
                {
                    exception.Redirect();
                }
                finally
                {
                    _request.Dispose();
                    response?.Dispose();
                }
            }
        }
    }
}
