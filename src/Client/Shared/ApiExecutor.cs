// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared
{
    public class ApiExecutor<T> : ComponentBase, IDisposable, IApiExecutor
    {
        private CancellationTokenSource? _cts;
        private bool _disposedValue;

        [Parameter] public Func<IApiClientOperation<T>> Operation { get; set; } = null!;
        [Parameter] public bool ExecuteOnInitialized { get; set; } = true;
        [Parameter] public RenderFragment? NotStarted { get; set; }
        [Parameter] public RenderFragment? Running { get; set; }
        [Parameter] public RenderFragment<T>? Success { get; set; }
        [Parameter] public RenderFragment<T>? ChildContent { get; set; }
        [Parameter] public RenderFragment? Failure { get; set; }

        protected Task? Task { get; private set; }
        protected T? Value { get; private set; }
        protected ProblemDetails? Problem { get; private set; }

        public ValueTask StartAsync()
        {
            if (Task is null)
            {
                SetTask();
            }

            return new(Task);
        }

        public ValueTask RestartAsync()
        {
            Reset();
            return StartAsync();
        }

        public void Reset()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            Task = null;
            Problem = null;
            Value = default;
            StateHasChanged();
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected override void OnParametersSet()
        {
            if (Operation is null)
            {
                throw new ArgumentNullException(nameof(Operation));
            }
            if (Success is not null && ChildContent is not null)
            {
                throw new ArgumentException("Either Success or ChildContent may be set, but not both.");
            }
        }

        protected override void OnInitialized()
        {
            if (ExecuteOnInitialized)
            {
                Debug.Assert(Task is null);
                SetTask();
            }
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Task is null)
            {
                builder.AddContent(0, NotStarted);
            }
            else
            {
                ProblemDetails problem;
                switch (Task.Status)
                {
                    case TaskStatus.Created:
                    case TaskStatus.WaitingForActivation:
                    case TaskStatus.WaitingToRun:
                    case TaskStatus.Running:
                    case TaskStatus.WaitingForChildrenToComplete:
                        if (Running is not null)
                        {
                            builder.AddContent(0, Running);
                            return;
                        }
                        else
                        {
                            builder.OpenComponent<LoadingIndicator>(0);
                            builder.CloseComponent();
                            return;
                        }
                    case TaskStatus.RanToCompletion:
                        if (Value is not null)
                        {
                            builder.AddContent(0, Success ?? ChildContent, Value);
                            return;
                        }
                        else
                        {
                            problem = Problem ?? new()
                            {
                                Detail = "An unknown error has occurred"
                            };
                            break;
                        }
                    case TaskStatus.Canceled:
                        problem = new()
                        {
                            Detail = "The operation was canceled."
                        };
                        break;
                    case TaskStatus.Faulted:
                        problem = new()
                        {
                            Detail = Task.Exception!.Message
                        };
                        break;
                    default: throw new ArgumentOutOfRangeException(null, "Task returned an invalid status.");
                }

                if (Failure is not null)
                {
                    builder.OpenComponent<CascadingValue<ProblemDetails>>(0);
                    builder.AddAttribute(1, nameof(CascadingValue<ProblemDetails>.Value), problem);
                    builder.AddAttribute(2, nameof(CascadingValue<ProblemDetails>.ChildContent), Failure);
                    builder.CloseComponent();
                }
                else
                {
                    builder.OpenComponent<DefaultProblemView>(0);
                    builder.AddAttribute(1, nameof(DefaultProblemView.Problem), problem);
                    builder.CloseComponent();
                }
            }
        }

        protected virtual IApiClientOperation<T> ConfigureOperation(IApiClientOperation<T> operation)
        {
            return operation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Reset();
                }

                _disposedValue = true;
            }
        }

        [MemberNotNull(nameof(Task), nameof(_cts))]
        private void SetTask()
        {
            _cts = new();
            Task = ConfigureOperation(Operation())
                .OnSuccess(value => Value = value)
                .OnFailure(problem => Problem = problem)
                .ExecuteAsync(_cts.Token);
            Task.ContinueWith(_ => StateHasChanged(), TaskContinuationOptions.ExecuteSynchronously);
            StateHasChanged();
        }
    }
}