// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Mvc;

namespace ValhallaLootList.Server.Controllers;

public class OidcConfigurationController(IClientRequestParametersProvider clientRequestParametersProvider) : Controller
{
    public IClientRequestParametersProvider ClientRequestParametersProvider { get; } = clientRequestParametersProvider;

    [HttpGet("_configuration/{clientId}")]
    public IActionResult GetClientRequestParameters([FromRoute] string clientId)
    {
        var parameters = ClientRequestParametersProvider.GetClientParameters(HttpContext, clientId);
        return Ok(parameters);
    }
}
