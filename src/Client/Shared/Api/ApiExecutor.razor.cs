// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared
{
    public partial class ApiExecutor<T> : ComponentBase, IDisposable, IApiExecutor
    {
        private IApiClientOperation<T>? _activeOperation;
        private CancellationTokenSource? _activeOperationCts;
        private bool _disposedValue;

        [Parameter] public Func<IApiClientOperation<T>> Operation { get; set; } = null!;
        [Parameter] public bool ExecuteOnInitialized { get; set; } = true;
        [Parameter] public RenderFragment? NotStarted { get; set; }
        [Parameter] public RenderFragment? Running { get; set; }
        [Parameter] public RenderFragment<T>? Success { get; set; }
        [Parameter] public RenderFragment<T>? ChildContent { get; set; }
        [Parameter] public RenderFragment? Failure { get; set; }
        [Parameter] public bool BackgroundRefresh { get; set; }

        protected IApiClientOperation<T>? ActiveOperation
        {
            get => _activeOperation;
            set
            {
                if (_activeOperation != value)
                {
                    if (_activeOperation is not null)
                    {
                        _activeOperation.StatusChanged -= StateHasChanged;
                    }

                    if (_activeOperationCts is not null)
                    {
                        _activeOperationCts.Cancel();
                        _activeOperationCts.Dispose();
                        _activeOperationCts = null;
                    }

                    if (value is not null)
                    {
                        value.StatusChanged += StateHasChanged;
                    }

                    _activeOperation = value;
                }
            }
        }

        public Task StartAsync()
        {
            if (ActiveOperation is null)
            {
                ActiveOperation = CreateOperation();
                _activeOperationCts = new();
                ActiveOperation.ExecuteAsync(_activeOperationCts.Token);
                StateHasChanged();
            }

            if (ActiveOperation.Status == ApiOperationStatus.NotStarted)
            {
                return ActiveOperation.ExecuteAsync();
            }

            return ActiveOperation.Task;
        }

        public Task RestartAsync(bool? backgroundRefresh = null)
        {
            if ((backgroundRefresh ?? BackgroundRefresh) && ActiveOperation?.Status == ApiOperationStatus.Success)
            {
                return BackgroundRestartAsync();
            }
            else
            {
                ActiveOperation = null;
                return StartAsync();
            }
        }

        private Task BackgroundRestartAsync()
        {
            var op = CreateOperation();
            var cts = new CancellationTokenSource();

            op.StatusChanged += BackgroundStatusChanged;

            return op.ExecuteAsync(cts.Token);

            void BackgroundStatusChanged()
            {
                if (op.Status == ApiOperationStatus.Failure || op.Status == ApiOperationStatus.Success)
                {
                    op.StatusChanged -= BackgroundStatusChanged;
                    ActiveOperation = op;
                    _activeOperationCts = cts;
                    StateHasChanged();
                }
            }
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
            if (ExecuteOnInitialized && ActiveOperation is null)
            {
                ActiveOperation = CreateOperation();
            }
        }

        protected virtual IApiClientOperation<T> ConfigureOperation(IApiClientOperation<T> operation)
        {
            operation.StatusChanged += StateHasChanged;
            return operation;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    ActiveOperation = null;
                }

                _disposedValue = true;
            }
        }

        private IApiClientOperation<T> CreateOperation()
        {
            return ConfigureOperation(Operation());
        }
    }
}