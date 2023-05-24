// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Discord;

public class MessageSender
{
    private readonly ApplicationDbContext _context;
    private readonly DiscordClientProvider _discordClientProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly record struct DropData(uint ItemId, string ItemName, string? WinnerName, bool Disenchanted);

    public MessageSender(ApplicationDbContext context, DiscordClientProvider discordClientProvider, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _discordClientProvider = discordClientProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendKillMessageAsync(long raidId, string encounterId, byte trashIndex)
    {
        var kill = await _context.EncounterKills.FindAsync(encounterId, raidId, trashIndex) ?? throw new Exception("Kill not found.");

        var drops = new List<DropData>();
        string? teamName = null, encounterName = null;

        await foreach (var d in _context.Drops
            .AsNoTracking()
            .Where(d => d.EncounterKillEncounterId == encounterId && d.EncounterKillRaidId == raidId && d.EncounterKillTrashIndex == trashIndex)
            .Select(d => new { d.ItemId, ItemName = d.Item.Name, WinnerName = (string?)d.Winner!.Name, TeamName = d.EncounterKill.Raid.RaidTeam.Name, EncounterName = d.EncounterKill.Encounter.Name, d.Disenchanted })
            .AsAsyncEnumerable())
        {
            teamName = d.TeamName;
            encounterName = d.EncounterName;
            drops.Add(new(d.ItemId, d.ItemName, d.WinnerName, d.Disenchanted));
        }

        if (teamName is null || encounterName is null)
        {
            (teamName, encounterName) = await _context.EncounterKills.AsNoTracking()
                .Where(ek => ek.RaidId == raidId && ek.EncounterId == encounterId && ek.TrashIndex == trashIndex)
                .Select(ek => new ValueTuple<string, string>(ek.Raid.RaidTeam.Name, ek.Encounter.Name))
                .FirstAsync();
        }

        var message = await _discordClientProvider.SendOrUpdatePublicNotificationAsync(
            kill.DiscordMessageId,
            m => ConfigureKillMessage(m, raidId, kill.KilledAt, teamName, encounterName, kill.EncounterId, drops));

        if (message is not null)
        {
            var messageId = (long)message.Id;

            if (messageId != kill.DiscordMessageId)
            {
                kill.DiscordMessageId = messageId;
                await _context.SaveChangesAsync();
            }
        }
    }

    public async Task DeleteKillMessageAsync(long messageId)
    {
        if (messageId > 0)
        {
            await _discordClientProvider.DeletePublicNotificationAsync(messageId);
        }
    }

    public async Task SendNewApplicationMessagesAsync(Character character, List<long> submittedTeamIds)
    {
        await foreach (var leader in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => submittedTeamIds.Contains(rtl.RaidTeamId))
            .Select(rtl => new { rtl.UserId, rtl.RaidTeamId, TeamName = rtl.RaidTeam.Name })
            .AsAsyncEnumerable())
        {
            await _discordClientProvider.SendDmAsync(
                leader.UserId,
                $"You have a new application to {leader.TeamName} from {character.Name}. ({character.Race.GetDisplayName()} {character.Class.GetDisplayName()})");
        }
    }

    public async Task SendApprovedApplicationMessagesAsync(Character character, RaidTeam team, string? message)
    {
        await NotifyApplicationStateChanged(character, team, approved: true, message);
    }

    public async Task SendDeniedApplicationMessagesAsync(Character character, RaidTeam team, string? message)
    {
        await NotifyApplicationStateChanged(character, team, approved: false, message);
    }

    public async Task SendNewListMessagesAsync(CharacterLootList list, RaidTeam team)
    {
        if (team is null)
        {
            throw new ArgumentException("Loot list's character must be assigned to a team.");
        }

        var dm = $"{list.Character.Name} ({list.Character.Race.GetDisplayName()} {list.MainSpec.GetDisplayName(includeClassName: true)}) has submitted a new phase {list.Phase} loot list for team {team.Name}.";

        await foreach (var leaderId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.RaidTeamId == team.Id)
            .Select(rtl => rtl.UserId)
            .AsAsyncEnumerable())
        {
            await _discordClientProvider.SendDmAsync(leaderId, dm);
        }
    }

    public async Task SendApprovedListMessagesAsync(Character character, CharacterLootList list, string? message)
    {
        await NotifyListStateChanged(character, list, approved: true, message);
    }

    public async Task SendDeniedListMessagesAsync(Character character, CharacterLootList list, string? message)
    {
        await NotifyListStateChanged(character, list, approved: false, message);
    }

    public async Task SendGemEnchantMessagesAsync(long teamId, long characterId, bool enchanted, string? message)
    {
        await SendBonusMessageAsync("gem & enchant", teamId, characterId, enchanted, message);
    }

    public async Task SendPreparedMessagesAsync(long teamId, long characterId, bool prepared, string? message)
    {
        await SendBonusMessageAsync("prepared", teamId, characterId, prepared, message);
    }

    private async Task NotifyApplicationStateChanged(Character character, RaidTeam team, bool approved, string? message)
    {
        if (character.OwnerId > 0)
        {
            var sb = new StringBuilder("Your application to ")
                .Append(team.Name)
                .Append(" for ")
                .Append(character.Name)
                .Append(" was ")
                .Append(approved ? "approved!" : "rejected.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine().Append("<@").Append(GetUserDiscordId()).Append("> said:");

                foreach (var line in message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine().Append("> ").Append(line);
                }
            }

            if (approved)
            {
                await _discordClientProvider.AddRoleAsync(character.OwnerId.Value, team.Name, "Accepted onto the raid team.");
            }

            await _discordClientProvider.SendDmAsync(character.OwnerId.Value, sb.ToString());
        }
    }

    private async Task NotifyListStateChanged(Character character, CharacterLootList list, bool approved, string? message)
    {
        if (character.OwnerId > 0)
        {
            var sb = new StringBuilder("Your phase")
                .Append(list.Phase)
                .Append(" loot list for ")
                .Append(character.Name)
                .Append(" was ")
                .Append(approved ? "approved!" : "rejected.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                sb.AppendLine().Append("<@").Append(GetUserDiscordId()).Append("> said:");

                foreach (var line in message.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine().Append("> ").Append(line);
                }
            }

            await _discordClientProvider.SendDmAsync(character.OwnerId.Value, sb.ToString());
        }
    }

    private long GetUserDiscordId()
    {
        return _httpContextAccessor.HttpContext?.User?.GetDiscordId() ?? throw new InvalidOperationException("Couldn't access the calling user's discord id.");
    }

    private async Task SendBonusMessageAsync(string bonusName, long teamId, long characterId, bool awarded, string? message)
    {
        var messageTargets = new HashSet<long>(capacity: 4);

        var character = await _context.Characters.Where(c => c.Id == characterId).Select(c => new { c.Name, c.OwnerId }).FirstAsync();

        if (character.OwnerId > 0)
        {
            messageTargets.Add(character.OwnerId.Value);
        }

        await foreach (var leaderId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.RaidTeamId == teamId)
            .Select(rtl => rtl.UserId)
            .AsAsyncEnumerable())
        {
            messageTargets.Add(leaderId);
        }

        if (messageTargets.Count > 0)
        {
            var sb = new StringBuilder(character.Name)
                .Append(" has had their ")
                .Append(bonusName)
                .Append(" bonus ")
                .Append(awarded ? "given" : "removed")
                .Append(" by <@")
                .Append(GetUserDiscordId())
                .Append(">.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                foreach (var line in message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine().Append("> ").Append(line);
                }
            }

            var completeMessage = sb.ToString();

            foreach (var discordId in messageTargets)
            {
                await _discordClientProvider.SendDmAsync(discordId, completeMessage);
            }
        }
    }

    public async Task SendMembershipStatusMessagesAsync(long teamId, long characterId, RaidMemberStatus? status, string? message)
    {
        var messageTargets = new HashSet<long>(capacity: 4);

        var character = await _context.Characters.Where(c => c.Id == characterId).Select(c => new { c.Name, c.OwnerId }).FirstAsync();

        if (character.OwnerId > 0)
        {
            messageTargets.Add(character.OwnerId.Value);
        }

        await foreach (var leaderId in _context.RaidTeamLeaders
            .AsNoTracking()
            .Where(rtl => rtl.RaidTeamId == teamId)
            .Select(rtl => rtl.UserId)
            .AsAsyncEnumerable())
        {
            messageTargets.Add(leaderId);
        }

        if (messageTargets.Count > 0)
        {
            var sb = new StringBuilder(character.Name)
                .Append(" has had their membership status ");

            if (status is null)
            {
                sb.Append(" set to automatic");
            }
            else
            {
                sb.Append("manually overridden to ")
                    .Append(status switch
                    {
                        RaidMemberStatus.Member => "member",
                        RaidMemberStatus.HalfTrial => "half trial",
                        RaidMemberStatus.FullTrial => "full trial",
                        _ => "automatic",
                    });
            }

            sb.Append(" by <@")
                .Append(GetUserDiscordId())
                .Append(">.");

            if (!string.IsNullOrWhiteSpace(message))
            {
                foreach (var line in message.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    sb.AppendLine().Append("> ").Append(line);
                }
            }

            var completeMessage = sb.ToString();

            foreach (var discordId in messageTargets)
            {
                await _discordClientProvider.SendDmAsync(discordId, completeMessage);
            }
        }
    }

    private void ConfigureKillMessage(
        DiscordMessageBuilder message,
        long raidId,
        DateTimeOffset killedAt,
        string teamName,
        string encounterName,
        string encounterId,
        List<DropData> drops)
    {
        var request = _httpContextAccessor.HttpContext?.Request ?? throw new InvalidOperationException("Messages must be sent within an http request.");

        var builder = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#3949AB"))
            .WithAuthor(name: teamName, url: $"{request.Scheme}://{request.Host}{request.PathBase}/teams/{teamName}")
            .WithUrl($"{request.Scheme}://{request.Host}{request.PathBase}/raids/{raidId}")
            .WithDescription($"Killed <t:{killedAt.ToUnixTimeSeconds()}:f>");

        if (encounterId.Contains("trash"))
        {
            builder.WithTitle("Trash Drops");
        }
        else
        {
            builder.WithTitle($":skull_crossbones: {encounterName} :skull_crossbones:")
                .WithThumbnail($"https://valhallalootliststorage.blob.core.windows.net/encounters/{encounterId}.png", height: 64, width: 128);
        }

        var itemsBuilder = new StringBuilder();

        foreach (var drop in drops.OrderBy(x => x.ItemName))
        {
            itemsBuilder.Append('[').Append(drop.ItemName).Append("](https://www.wowhead.com/wotlk/item=").Append(drop.ItemId).Append("): ");

            if (drop.Disenchanted)
            {
                itemsBuilder.AppendLine("Disenchanted");
            }
            else if (drop.WinnerName?.Length > 0)
            {
                itemsBuilder.Append("Awarded to ").AppendLine(drop.WinnerName);
            }
            else
            {
                itemsBuilder.AppendLine("Not awarded");
            }
        }

        builder.AddField("Loot", itemsBuilder.ToString());

        message.Embed = builder.Build();
    }
}
