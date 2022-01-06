// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared;

public class WizardSection : ComponentBase
{
    private string? _continueText;
    private bool _continueTextChanged;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public Func<bool>? CanContinue { get; set; }

    [Parameter]
    public string? ContinueText
    {
        get => _continueText;
        set
        {
            if (_continueText != value)
            {
                _continueText = value;
                _continueTextChanged = true;
            }
        }
    }

    [CascadingParameter] public WizardDialog WizardDialog { get; set; } = null!;

    protected override void OnInitialized()
    {
        if (WizardDialog is null) throw new Exception("WizardSection must be within a WizardDialog.");
        WizardDialog.AddSection(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (_continueTextChanged)
        {
            _continueTextChanged = false;
            WizardDialog.NotifyStateChanged();
        }
    }
}
