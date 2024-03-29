﻿// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using MudBlazor;

namespace ValhallaLootList.Client.Shared;

public static class DefaultMudTheme
{
    public static MudTheme Value { get; } = new()
    {
        Palette =
        {
            Primary = "#3949AB",
            Secondary = Colors.Blue.Accent1,
            Tertiary = Colors.Orange.Darken3,
            AppbarBackground = "#3949AB"
        }
    };
}
