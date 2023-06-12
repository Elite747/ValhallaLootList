// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor.Utilities;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared;

public partial class ItemLink
{
    private Item? _item;
    private bool _loading, _failed, _disposed;

    protected string? Classname => new CssBuilder()
        .AddClass($"q{Quality}", Colorize)
        .AddClass("text-bracket", Bracketize)
        .AddClass(Class)
        .Build();

    protected string? CompleteStyle => new StyleBuilder()
        .AddStyle(Style)
        .AddStyle("cursor", "pointer", !LinkEnabled)
        .Build();

    protected string? Target => LinkEnabled ? "_blank" : null;

    protected string? Href
    {
        get
        {
            if (LinkEnabled)
            {
                uint? id = Context?.Id ?? Id;

                if (id > 0)
                {
                    return $"https://www.wowhead.com/wotlk/item={id}";
                }
            }

            return "#";
        }
    }

    protected string? DataWowhead
    {
        get
        {
            uint? id = Context?.Id ?? Id;

            if (id > 0)
            {
                return $"item={id}&domain=wotlk";
            }
            return null;
        }
    }

    protected int Quality
    {
        get
        {
            if (Context?.Item is not null)
            {
                return Context.Item.Quality;
            }
            if (_item is not null)
            {
                return _item.Quality;
            }
            return 0;
        }
    }

    [Parameter] public uint? Id { get; set; }

    [Parameter] public bool Bracketize { get; set; }

    [Parameter] public bool Colorize { get; set; }

    [Parameter] public string? PlaceholderText { get; set; }

    [Parameter] public bool LinkEnabled { get; set; }

    [CascadingParameter] public ItemLinkContext? Context { get; set; }

    [Inject] public ItemProvider Items { get; set; } = null!;

    protected override async Task OnParametersSetAsync()
    {
        if (Context is null)
        {
            _loading = true;
            _failed = false;
            if (Id > 0)
            {
                try
                {
                    _item = await Items.GetItemAsync(Id.Value);
                }
                catch
                {
                    _failed = true;
                }
            }
            else
            {
                _failed = true;
            }
            _loading = false;
        }
        else
        {
            _item = Context.Item;
        }
    }

    private async Task OnClickAsync()
    {
        if (LinkEnabled)
        {
            await JS.InvokeVoidAsync("open", Href, Target);
        }
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
            return WH.HideTooltipAsync();
        }
        return default;
    }
}
