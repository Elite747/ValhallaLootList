#pragma warning disable IDE0073 // The file header does not match the required text
// https://github.com/javiercn/BlazorAuthRoles/blob/master/Client/RolesClaimsPrincipalFactory.cs
#pragma warning restore IDE0073 // The file header does not match the required text

using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;

namespace ValhallaLootList.Client
{
    public class RolesClaimsPrincipalFactory : AccountClaimsPrincipalFactory<RemoteUserAccount>
    {
        public RolesClaimsPrincipalFactory(IAccessTokenProviderAccessor accessor) : base(accessor)
        {
        }

        public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account, RemoteAuthenticationUserOptions options)
        {
            var user = await base.CreateUserAsync(account, options);
            if (user.Identity?.IsAuthenticated == true)
            {
                var identity = (ClaimsIdentity)user.Identity;

                foreach (var claim in identity.Claims.ToList())
                {
                    if (account.AdditionalProperties.TryGetValue(claim.Type, out var claimObj) && claimObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
                    {
                        identity.RemoveClaim(claim);

                        foreach (var arrayElement in jsonElement.EnumerateArray())
                        {
                            identity.AddClaim(new Claim(claim.Type, arrayElement.GetString() ?? string.Empty));
                        }
                    }
                }
            }

            return user;
        }
    }
}
