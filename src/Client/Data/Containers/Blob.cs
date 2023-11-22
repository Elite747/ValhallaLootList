// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Diagnostics.CodeAnalysis;

namespace ValhallaLootList.Client.Data.Containers;

public class Blob(string name, string url)
{
    public string Name { get; } = name;

    public string Url { get; } = url;

    public string GetTitle()
    {
        var nameParts = GetNameParts();

        if (nameParts.Length > 0)
        {
            return nameParts[0];
        }

        return string.Empty;
    }

    public string GetLogoName()
    {
        var nameParts = GetNameParts();

        if (nameParts.Length == 3)
        {
            return nameParts[1];
        }

        return string.Empty;
    }

    public bool TryGetLogoUrl([NotNullWhen(true)] out string? logoUrl)
    {
        var logoName = GetLogoName();

        if (logoName.Length > 0)
        {
            logoUrl = "https://valhallalootliststorage.blob.core.windows.net/logos/" + logoName + "AlphaCropped.png";
            return true;
        }
        logoUrl = null;
        return false;
    }

    private string[] GetNameParts()
    {
        return Name?.Split('.', '-') ?? [];
    }
}
