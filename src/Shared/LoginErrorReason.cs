// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList
{
    public enum LoginErrorReason
    {
        Unknown = 0,
        FromDiscord = 1,
        FromLoginProvider = 2,
        LockedOut = 3,
        FromAccountCreation = 4,
        NotInGuild = 5,
        NotAMember = 6
    }
}
