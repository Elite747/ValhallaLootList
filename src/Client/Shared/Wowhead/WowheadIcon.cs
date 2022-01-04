// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared;

public class WowheadIcon : WowIcon
{
    [Parameter] public string IconId { get; set; } = string.Empty;

    protected override string GetIconId() => IconId;

    protected override bool IconReady() => IconId?.Length > 0;
}
