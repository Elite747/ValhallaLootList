// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Mvc;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers;

public static class DiscordMessageBuilderExtensions
{
    public static void ConfigureKillMessage(
        this DiscordMessageBuilder message,
        HttpRequest request,
        IUrlHelper url,
        EncounterKill kill,
        string teamName,
        string encounterName,
        List<(uint, string, string?)> drops)
    {
        ConfigureKillMessage(message, request, url, kill.RaidId, kill.KilledAt, teamName, encounterName, drops);
    }

    public static void ConfigureKillMessage(
        this DiscordMessageBuilder message,
        HttpRequest request,
        IUrlHelper url,
        long raidId,
        DateTimeOffset killedAt,
        string teamName,
        string encounterName,
        List<(uint, string, string?)> drops)
    {
        var builder = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor("#8E24AA"))
            .WithTitle($"{teamName} :skull_crossbones:{encounterName}:skull_crossbones:")
            .WithUrl(request.Scheme + "://" + request.Host + url.Content($"~/raids/{raidId}"))
            .WithDescription($"{teamName} killed {encounterName} on {killedAt:D} at {killedAt:t}")
            .WithTimestamp(killedAt);

        var itemsBuilder = new StringBuilder();

        foreach (var (itemId, itemName, winnerName) in drops.OrderBy(x => x.Item2))
        {
            itemsBuilder.Append('[').Append(itemName).Append("](https://tbc.wowhead.com/item=").Append(itemId).Append("): ");

            if (winnerName?.Length > 0)
            {
                itemsBuilder.Append("Awarded to ").AppendLine(winnerName);
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
