// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

namespace ValhallaLootList.Server.Discord;

public class DiscordServiceOptions
{
    public bool SuppressOutgoingMessages { get; set; }

    public string? BotToken { get; set; }

    public long GuildId { get; set; }

    public long PublicNotificationChannelId { get; set; }

    public long OfficerNotificationChannelId { get; set; }

    public long MemberRoleId { get; set; }

    public long AdminRoleId { get; set; }

    public long RaidLeaderRoleId { get; set; }

    public long LootMasterRoleId { get; set; }

    public long RecruiterRoleId { get; set; }

    public long LeadershipRoleId { get; set; }
}
