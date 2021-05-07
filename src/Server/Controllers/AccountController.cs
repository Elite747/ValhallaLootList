// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ValhallaLootList.Server.Data;
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DiscordClientProvider _discordClientProvider;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<AppUser> signInManager, DiscordClientProvider discordClientProvider, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _discordClientProvider = discordClientProvider;
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
            IdentityResult? identityResult;

            returnUrl ??= Url.Content("~/");
            if (remoteError != null)
            {
                return LocalRedirect("~/loginerror/" + LoginErrorReason.FromDiscord + "/" + remoteError);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null || !long.TryParse(info.ProviderKey, out var discordId))
            {
                return LocalRedirect("~/loginerror/" + LoginErrorReason.FromLoginProvider);
            }

            DiscordMember? guildMember;
            try
            {
                guildMember = await _discordClientProvider.GetMemberAsync(discordId);
                if (guildMember is null)
                {
                    return LocalRedirect("~/loginerror/" + LoginErrorReason.NotInGuild);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "A problem occurred while trying to get guild member information.");
                return LocalRedirect("~/loginerror/" + LoginErrorReason.FromLoginProvider);
            }

            if (!_discordClientProvider.HasMemberRole(guildMember))
            {
                return LocalRedirect("~/loginerror/" + LoginErrorReason.NotAMember);
            }

            // Sign in the user with this external login provider if the user already has a login.
            //var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            var user = await _signInManager.UserManager.FindByIdAsync(info.ProviderKey);

            if (user is not null)
            {
                if (await _signInManager.UserManager.IsLockedOutAsync(user))
                {
                    return LocalRedirect("~/loginerror/" + LoginErrorReason.LockedOut);
                }

                await _signInManager.SignInAsync(user, isPersistent: true, authenticationMethod: info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

                return LocalRedirect(returnUrl);
            }
            else
            {
                user = new AppUser
                {
                    Id = long.Parse(info.ProviderKey),
                    UserName = guildMember.DisplayName
                };

                identityResult = await _signInManager.UserManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await _signInManager.UserManager.AddLoginAsync(user, info);

                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        await _signInManager.SignInAsync(user, isPersistent: true, authenticationMethod: info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }

                _logger.LogError("User creation failed: {Result}", identityResult);
                return LocalRedirect("~/loginerror/" + LoginErrorReason.FromAccountCreation);
            }
        }
    }
}
