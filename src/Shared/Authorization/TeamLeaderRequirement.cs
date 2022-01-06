// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;

namespace ValhallaLootList;

public class TeamLeaderRequirement : IAuthorizationRequirement
{
    public TeamLeaderRequirement(bool allowAdmin, bool allowRaidLeader, bool allowLootMaster, bool allowRecruiter)
    {
        AllowAdmin = allowAdmin;
        AllowRaidLeader = allowRaidLeader;
        AllowLootMaster = allowLootMaster;
        AllowRecruiter = allowRecruiter;
    }

    public bool AllowAdmin { get; }

    public bool AllowRaidLeader { get; }

    public bool AllowLootMaster { get; }

    public bool AllowRecruiter { get; }
}
