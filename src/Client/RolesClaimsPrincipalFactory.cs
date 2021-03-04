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
                var roleClaims = identity.FindAll(options.RoleClaim).ToList();
                if (roleClaims?.Count > 0)
                {
                    foreach (var existingClaim in roleClaims)
                    {
                        identity.RemoveClaim(existingClaim);
                    }

                    var rolesElem = account.AdditionalProperties[options.RoleClaim];
                    if (rolesElem is JsonElement roles)
                    {
                        if (roles.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var role in roles.EnumerateArray())
                            {
                                identity.AddClaim(new Claim(options.RoleClaim, role.GetString() ?? string.Empty));
                            }
                        }
                        else
                        {
                            identity.AddClaim(new Claim(options.RoleClaim, roles.GetString() ?? string.Empty));
                        }
                    }
                }
            }

            return user;
        }
    }
}
