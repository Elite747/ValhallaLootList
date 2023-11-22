// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using System.Text.Json.Serialization;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Serialization;

public class PriorityBonusConverter : JsonConverter<PriorityBonusDto>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(PriorityBonusDto).IsAssignableFrom(typeToConvert);
    }

    public override PriorityBonusDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.PropertyName)
        {
            throw new JsonException();
        }

        string? propertyName = reader.GetString();
        if (propertyName != nameof(PriorityBonusDto.Type))
        {
            throw new JsonException();
        }

        reader.Read();
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException();
        }

        var type = reader.GetString() ?? string.Empty;
        var bonus = type switch
        {
            PriorityBonusTypes.Absence => new AbsencePriorityBonusDto(),
            PriorityBonusTypes.Donation => new DonationPriorityBonusDto(),
            PriorityBonusTypes.Lost => new LossPriorityBonusDto(),
            PriorityBonusTypes.Trial => new MembershipPriorityBonusDto(),
            _ => new PriorityBonusDto()
        };

        bonus.Type = type;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return bonus;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case nameof(PriorityBonusDto.Value):
                        bonus.Value = reader.GetInt32();
                        break;
                    case nameof(AbsencePriorityBonusDto.Absences) when bonus is AbsencePriorityBonusDto attendancePriorityBonus:
                        attendancePriorityBonus.Absences = reader.GetInt32();
                        break;
                    case nameof(DonationPriorityBonusDto.DonationTickets) when bonus is DonationPriorityBonusDto donationPriorityBonus:
                        donationPriorityBonus.DonationTickets = reader.GetInt32();
                        break;
                    case nameof(LossPriorityBonusDto.TimesSeen) when bonus is LossPriorityBonusDto lossPriorityBonus:
                        lossPriorityBonus.TimesSeen = reader.GetInt32();
                        break;
                    case nameof(MembershipPriorityBonusDto.Status) when bonus is MembershipPriorityBonusDto membershipPriorityBonus:
                        membershipPriorityBonus.Status = (RaidMemberStatus)reader.GetInt32();
                        break;
                }
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, PriorityBonusDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(nameof(PriorityBonusDto.Type), value.Type);
        writer.WriteNumber(nameof(PriorityBonusDto.Value), value.Value);

        switch (value)
        {
            case AbsencePriorityBonusDto bonus:
                writer.WriteNumber(nameof(AbsencePriorityBonusDto.Absences), bonus.Absences);
                break;
            case DonationPriorityBonusDto bonus:
                writer.WriteNumber(nameof(DonationPriorityBonusDto.DonationTickets), bonus.DonationTickets);
                break;
            case LossPriorityBonusDto bonus:
                writer.WriteNumber(nameof(LossPriorityBonusDto.TimesSeen), bonus.TimesSeen);
                break;
            case MembershipPriorityBonusDto bonus:
                writer.WriteNumber(nameof(MembershipPriorityBonusDto.Status), (int)bonus.Status);
                break;
        }

        writer.WriteEndObject();
    }
}
