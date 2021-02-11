// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValhallaLootList.Server.Data;

namespace ValhallaLootList.Server.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<AppUser> signInManager, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = "/")
        {
            var redirect = Url.Action(nameof(Callback), new { returnUrl });
            return Challenge(_signInManager.ConfigureExternalAuthenticationProperties("Discord", redirect), "Discord");
        }

        [HttpGet]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return LocalRedirect(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> Callback(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                return LocalRedirect("~/loginerror?reason=1&remoteError=" + remoteError);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return LocalRedirect("~/loginerror?reason=2");
            }

            // TODO: Check with bot if user is on-server and has roles.

            // Sign in the user with this external login provider if the user already has a login.
            //var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            var user = await _signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user is not null)
            {
                if (await _signInManager.UserManager.IsLockedOutAsync(user))
                {
                    return LocalRedirect("/loginerror?reason=3");
                }

                var existingClaims = await _signInManager.UserManager.GetClaimsAsync(user);
                var newClaims = new List<Claim>();

                foreach (var claim in info.Principal.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)))
                {
                    var existing = existingClaims.FirstOrDefault(c => c.Type == claim.Type);

                    if (existing != null)
                    {
                        if (existing.Value != claim.Value)
                        {
                            await _signInManager.UserManager.ReplaceClaimAsync(user, existing, claim);
                        }
                    }
                    else
                    {
                        newClaims.Add(claim);
                    }
                }

                if (newClaims.Count != 0)
                {
                    await _signInManager.UserManager.AddClaimsAsync(user, newClaims);
                }

                await _signInManager.SignInAsync(user, isPersistent: true, authenticationMethod: info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

                return LocalRedirect(returnUrl);
            }
            else
            {
                // todo: redirect if user is not in the guild.

                /*
                Not actually utilizing the UserName property since that is read by Discord, which is far less restrictive with their naming scheme than
                the default asp.net identity username policy. Rather than trying to match Discord's policy, set the username to the unique discord user
                key to satisfy asp.net identity requirements for user creation and do not use AppUser.UserName.
                */
                user = new AppUser { UserName = info.ProviderKey };

                var identityResult = await _signInManager.UserManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await _signInManager.UserManager.AddLoginAsync(user, info);

                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        identityResult = await _signInManager.UserManager.AddClaimsAsync(user, info.Principal.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)));

                        if (identityResult.Succeeded)
                        {
                            await _signInManager.SignInAsync(user, isPersistent: true, info.LoginProvider);

                            return LocalRedirect(returnUrl);
                        }
                    }
                }

                return LocalRedirect("~/loginerror?reason=4");
            }
        }
    }
}
