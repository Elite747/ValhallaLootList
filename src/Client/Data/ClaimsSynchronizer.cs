// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ValhallaLootList.Client.Data
{
    public class ClaimsSynchronizer
    {
        private readonly IJSRuntime _js;

        public ClaimsSynchronizer(IJSRuntime js)
        {
            _js = js;
        }

        public Task AddAsync(ClaimsPrincipal user, string type, string value, CancellationToken cancellationToken = default)
        {
            var claimValues = user.FindAll(type).Select(claim => claim.Value).ToList();

            if (!claimValues.Contains(value))
            {
                if (user.Identity is ClaimsIdentity identity)
                {
                    identity.AddClaim(new(type, value));
                }

                claimValues.Add(value);

                return UpdateUserRolesAsync(type, claimValues, cancellationToken).AsTask();
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(ClaimsPrincipal user, string type, string value, CancellationToken cancellationToken = default)
        {
            var claims = user.FindAll(type).ToList();

            var claim = claims.Find(c => c.Value == value);

            if (claim is not null)
            {
                if (user.Identity is ClaimsIdentity identity)
                {
                    identity.RemoveClaim(claim);
                }

                claims.Remove(claim);

                return UpdateUserRolesAsync(type, claims.ConvertAll(claim => claim.Value), cancellationToken).AsTask();
            }

            return Task.CompletedTask;
        }

        private ValueTask UpdateUserRolesAsync(string type, List<string> values, CancellationToken cancellationToken)
        {
            object claims = values.Count switch
            {
                0 => null!,
                1 => values[0],
                _ => values
            };

            return _js.InvokeVoidAsync("valhallaLootList.updateUserRoles", cancellationToken, type, claims);
        }
    }
}
