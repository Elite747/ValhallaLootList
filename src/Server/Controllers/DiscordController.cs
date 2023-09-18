// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ValhallaLootList.Server.Controllers;

[Route("[controller]"), AllowAnonymous]
public class DiscordController : ControllerBase
{
    public IActionResult Get()
    {
        return Redirect("https://discord.com/invite/DTrFTaG");
    }
}
