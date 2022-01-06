// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Text.Json;
using System.Text.Json.Serialization;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Serialization;

public class PriorityBonusConverter : JsonConverter<PriorityBonusDto>
{
    public override bool CanConvert(Type typeToConvert) => typeof(PriorityBonusDto).IsAssignableFrom(typeToConvert);

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
            PriorityBonusTypes.Attendance => new AttendancePriorityBonusDto(),
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
                    case nameof(AttendancePriorityBonusDto.AttendancePerPoint) when bonus is AttendancePriorityBonusDto attendancePriorityBonus:
                        attendancePriorityBonus.AttendancePerPoint = reader.GetInt32();
                        break;
                    case nameof(AttendancePriorityBonusDto.Attended) when bonus is AttendancePriorityBonusDto attendancePriorityBonus:
                        attendancePriorityBonus.Attended = reader.GetInt32();
                        break;
                    case nameof(AttendancePriorityBonusDto.ObservedAttendances) when bonus is AttendancePriorityBonusDto attendancePriorityBonus:
                        attendancePriorityBonus.ObservedAttendances = reader.GetInt32();
                        break;
                    case nameof(DonationPriorityBonusDto.DonatedCopper) when bonus is DonationPriorityBonusDto donationPriorityBonus:
                        donationPriorityBonus.DonatedCopper = reader.GetInt64();
                        break;
                    case nameof(DonationPriorityBonusDto.RequiredDonations) when bonus is DonationPriorityBonusDto donationPriorityBonus:
                        donationPriorityBonus.RequiredDonations = reader.GetInt32();
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
            case AttendancePriorityBonusDto bonus:
                writer.WriteNumber(nameof(AttendancePriorityBonusDto.AttendancePerPoint), bonus.AttendancePerPoint);
                writer.WriteNumber(nameof(AttendancePriorityBonusDto.Attended), bonus.Attended);
                writer.WriteNumber(nameof(AttendancePriorityBonusDto.ObservedAttendances), bonus.ObservedAttendances);
                break;
            case DonationPriorityBonusDto bonus:
                writer.WriteNumber(nameof(DonationPriorityBonusDto.DonatedCopper), bonus.DonatedCopper);
                writer.WriteNumber(nameof(DonationPriorityBonusDto.RequiredDonations), bonus.RequiredDonations);
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
