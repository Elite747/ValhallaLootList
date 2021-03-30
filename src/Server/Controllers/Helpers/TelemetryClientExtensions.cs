// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.ApplicationInsights;

namespace ValhallaLootList.Server.Controllers
{
    public static class TelemetryClientExtensions
    {
        public static void TrackEvent(this TelemetryClient telemetry, string eventName, ClaimsPrincipal initiator, Action<IDictionary<string, string>>? configure = null)
        {
            var properties = new Dictionary<string, string>
            {
                ["Initiator"] = initiator.Identity?.Name ?? string.Empty,
                ["InitiatorId"] = initiator.GetDiscordId()?.ToString() ?? string.Empty,
            };

            configure?.Invoke(properties);

            telemetry.TrackEvent(eventName, properties);
        }
    }
}
