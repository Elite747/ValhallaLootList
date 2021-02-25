// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ValhallaLootList.Client.Data.Items;

namespace ValhallaLootList.Client.Shared
{
    public partial class ItemLink : ComponentBase
    {
        private readonly ItemLinkContext _context = new();

        [Parameter]
        public uint? Id
        {
            get => _context.Id;
            set => _context.Id = value;
        }

        [Parameter]
        public IconSize? IconSize
        {
            get => _context.IconSize;
            set => _context.IconSize = value;
        }

        [Parameter]
        public bool Bracketize
        {
            get => _context.Bracketize;
            set => _context.Bracketize = value;
        }

        [Parameter]
        public bool Colorize
        {
            get => _context.Colorize;
            set => _context.Colorize = value;
        }

        [Parameter]
        public string? OverrideText
        {
            get => _context.OverrideText;
            set => _context.OverrideText = value;
        }

        [Parameter] public bool LinkEnabled { get; set; }

        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        [Inject] public ItemProvider Items { get; set; } = null!;

        protected override async Task OnParametersSetAsync()
        {
            _context.Item = null;
            _context.Failed = false;
            if (Id > 0)
            {
                try
                {
                    _context.Item = await Items.GetItemAsync(Id.Value);
                }
                catch
                {
                    _context.Item = null;
                }
                _context.Failed = _context.Item is null;
            }
        }
    }
}
