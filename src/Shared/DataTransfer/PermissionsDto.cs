// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.DataTransfer;

public class PermissionsDto
{
    public HashSet<long> Characters { get; set; } = new();

    public HashSet<long> Teams { get; set; } = new();
}
