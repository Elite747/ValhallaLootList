// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using ValhallaLootList.DataTransfer;

namespace ValhallaLootList.Client.Shared;

public class GuildMemberAvatar : ComponentBase
{
    [Parameter] public GuildMemberDto? Member { get; set; }

    [Parameter] public string? AvatarHash { get; set; }

    [Parameter] public long? Id { get; set; }

    [Parameter] public int? Discriminator { get; set; }

    [Parameter] public int? Size { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        //<img src="@GenerateSrc()" width="@Size" height="@Size" />
        builder.OpenElement(0, "img");
        builder.AddAttribute(1, "src", GenerateSrc());
        if (Size.HasValue)
        {
            builder.AddAttribute(2, "width", Size.Value);
            builder.AddAttribute(3, "height", Size.Value);
        }
        builder.CloseElement();
    }

    private string GenerateSrc()
    {
        const string defaultAvatarFormat = "https://cdn.discordapp.com/embed/avatars/{0}.png";
        const string avatarFormat = "https://cdn.discordapp.com/avatars/{0}/{1}.{2}";

        long? memberId = Id ?? Member?.Id;

        if (memberId.HasValue)
        {
            string? avatarHash = AvatarHash ?? Member?.Avatar;

            if (avatarHash?.Length > 0)
            {
                string type = avatarHash.StartsWith("a_") ? "gif" : "png";
                return string.Format(avatarFormat, memberId, avatarHash, type);
            }
        }

        return string.Format(defaultAvatarFormat, GetDiscriminator() % 5);
    }

    private int GetDiscriminator()
    {
        if (Discriminator.HasValue)
        {
            return Discriminator.Value;
        }

        if (int.TryParse(Member?.Discriminator, out int discriminator))
        {
            return discriminator;
        }

        return 1;
    }
}
