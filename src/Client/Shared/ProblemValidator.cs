// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ValhallaLootList.Client.Data;

namespace ValhallaLootList.Client.Shared;

public class ProblemValidator : ComponentBase
{
    private ValidationMessageStore? _messageStore;

    [CascadingParameter]
    public EditContext? CurrentEditContext { get; set; }

    protected override void OnInitialized()
    {
        if (CurrentEditContext is null)
        {
            throw new InvalidOperationException(
                $"{nameof(ProblemValidator)} requires a cascading " +
                $"parameter of type {nameof(EditContext)}. " +
                $"For example, you can use {nameof(ProblemValidator)} " +
                $"inside an {nameof(EditForm)}.");
        }

        _messageStore = new ValidationMessageStore(CurrentEditContext);

        CurrentEditContext.OnValidationRequested += (s, e) =>
            _messageStore.Clear();
        CurrentEditContext.OnFieldChanged += (s, e) =>
            _messageStore.Clear(e.FieldIdentifier);
    }

    public void DisplayErrors(ProblemDetails problem)
    {
        Debug.Assert(_messageStore is not null);
        Debug.Assert(CurrentEditContext is not null);

        if (problem.Errors?.Count > 0)
        {
            foreach (var (propertyName, errors) in problem.Errors)
            {
                _messageStore.Add(CurrentEditContext.Field(propertyName), errors);
            }
        }
        else
        {
            string? message = problem.Detail;

            if (string.IsNullOrEmpty(message))
            {
                if (problem.Title?.Length > 0 || problem.Status.HasValue)
                {
                    message = $"The server responded with status code {problem.Status} {problem.Title}";
                }
                else
                {
                    message = "An unknown error has occurred.";
                }
            }

            _messageStore.Add(CurrentEditContext.Field(string.Empty), message);
        }

        CurrentEditContext.NotifyValidationStateChanged();
    }

    public void ClearErrors()
    {
        _messageStore?.Clear();
        CurrentEditContext?.NotifyValidationStateChanged();
    }
}
