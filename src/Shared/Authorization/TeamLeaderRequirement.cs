// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList;

public class TeamLeaderRequirement(bool allowAdmin, bool allowRaidLeader, bool allowLootMaster, bool allowRecruiter) : IAuthorizationRequirement
{
    public bool AllowAdmin { get; } = allowAdmin;

    public bool AllowRaidLeader { get; } = allowRaidLeader;

    public bool AllowLootMaster { get; } = allowLootMaster;

    public bool AllowRecruiter { get; } = allowRecruiter;
}
