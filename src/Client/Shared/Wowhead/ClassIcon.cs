// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;

namespace ValhallaLootList.Client.Shared;

public class ClassIcon : WowIcon
{
    [Parameter] public Classes PlayerClass { get; set; }

    protected override string GetIconId()
    {
        return "class_" + PlayerClass.GetLowercaseName();
    }

    protected override string GetAltText()
    {
        return PlayerClass.ToString();
    }
}
