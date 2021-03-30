// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ValhallaLootList.Server.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize(AppPolicies.Member)]
    public abstract class ApiControllerV1 : ControllerBase
    {
    }
}
