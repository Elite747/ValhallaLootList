// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared;

public partial class ApiView : IDisposable
{
    private readonly List<IApiClientOperation> _operations = new();
    private bool _disposedValue, _contextChanged, _renderedSuccess;

    [Parameter]
    public IApiClientOperation Operation
    {
        get => _operations.Single();
        set
        {
            Operations = Single(value);

            static IEnumerable<IApiClientOperation> Single(IApiClientOperation c)
            {
                yield return c;
            }
        }
    }

    [Parameter]
    public IEnumerable<IApiClientOperation> Operations
    {
        get => _operations;
        set
        {
            foreach (var oldContext in _operations)
            {
                oldContext.StatusChanged -= OnContextStateChanged;
            }

            _operations.Clear();
            _contextChanged = true;
            _renderedSuccess = false;

            foreach (var newContext in value)
            {
                newContext.StatusChanged += OnContextStateChanged;
                _operations.Add(newContext);
            }
        }
    }

    [Parameter] public RenderFragment? NotStarted { get; set; }
    [Parameter] public RenderFragment? Running { get; set; }
    [Parameter] public RenderFragment? Success { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Failure { get; set; }
    [Parameter] public bool BackgroundRefresh { get; set; }
    [Parameter] public bool ExecuteImmediately { get; set; } = true;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected override void OnParametersSet()
    {
        if (Success is not null && ChildContent is not null)
        {
            throw new ArgumentException("Either Success or ChildContent may be set, but not both.");
        }

        if (_contextChanged)
        {
            _contextChanged = false;
            if (ExecuteImmediately)
            {
                foreach (var operation in _operations)
                {
                    if (operation.Status == ApiOperationStatus.NotStarted)
                    {
                        _ = operation.ExecuteAsync();
                    }
                }
            }
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                Operations = Array.Empty<IApiClientOperation>();
            }

            _disposedValue = true;
        }
    }

    private void OnContextStateChanged()
    {
        if (_operations.TrueForAll(c => c.Status == ApiOperationStatus.Success))
        {
            _renderedSuccess = true;
            StateHasChanged();
        }
        else if (!(_renderedSuccess && BackgroundRefresh))
        {
            StateHasChanged();
        }
    }

    private ProblemDetails FindFirstProblem()
    {
        foreach (var context in _operations)
        {
            if (context.Status == ApiOperationStatus.Failure)
            {
                return context.GetProblem();
            }
        }

        throw new InvalidOperationException("No execution contexts have a problem.");
    }
}
