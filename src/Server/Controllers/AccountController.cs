// Copyright (C) 2021 Donovan Sullivan
// GNU General Public License v3.0+ (see LICENSE or https://www.gnu.org/licenses/gpl-3.0.txt)

using System;
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
using ValhallaLootList.Server.Discord;

namespace ValhallaLootList.Server.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : ControllerBase
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly DiscordService _discordService;
        private readonly DiscordRoleMap _roles;
        private readonly ILogger<AccountController> _logger;

        public AccountController(SignInManager<AppUser> signInManager, DiscordService discordService, DiscordRoleMap roles, ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _discordService = discordService;
            _roles = roles;
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
                return LocalRedirect("~/loginerror?reason=1&remoteError=" + remoteError);
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null || !long.TryParse(info.ProviderKey, out var discordId))
            {
                return LocalRedirect("~/loginerror?reason=2");
            }

            GuildMemberInfo? guildMember;
            try
            {
                guildMember = await _discordService.GetMemberInfoAsync(discordId);
                if (guildMember?.RoleNames is null)
                {
                    return LocalRedirect("~/loginerror?reason=5");
                }
            }
            catch
            {
                return LocalRedirect("~/loginerror?reason=2");
            }

            if (!guildMember.RoleNames.Contains(_roles.Member))
            {
                return LocalRedirect("~/loginerror?reason=6");
            }

            // Sign in the user with this external login provider if the user already has a login.
            //var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: true, bypassTwoFactor: true);
            var user = await _signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            if (user is not null)
            {
                if (await _signInManager.UserManager.IsLockedOutAsync(user))
                {
                    return LocalRedirect("~/loginerror?reason=3");
                }

                if (user.UserName != guildMember.DisplayName)
                {
                    user.UserName = guildMember.DisplayName;
                    await _signInManager.UserManager.UpdateAsync(user);
                }

                var existingClaims = await _signInManager.UserManager.GetClaimsAsync(user);
                var newClaims = new List<Claim>();
                var removedClaims = new List<Claim>();

                foreach (var claim in info.Principal.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)))
                {
                    var existing = existingClaims.FirstOrDefault(c => c.Type == claim.Type);

                    if (existing != null)
                    {
                        if (existing.Value != claim.Value)
                        {
                            identityResult = await _signInManager.UserManager.ReplaceClaimAsync(user, existing, claim);

                            if (!identityResult.Succeeded)
                            {
                                return LocalRedirect("~/loginerror?reason=0");
                            }
                        }
                    }
                    else
                    {
                        newClaims.Add(claim);
                    }
                }

                foreach (var (appRole, discordRole) in _roles.AllRoles)
                {
                    var currentClaim = existingClaims.FirstOrDefault(claim => string.Equals(claim.Type, AppClaimTypes.Role) && string.Equals(claim.Value, appRole));

                    if (guildMember.RoleNames.Contains(discordRole))
                    {
                        if (currentClaim is null)
                        {
                            newClaims.Add(new Claim(AppClaimTypes.Role, appRole));
                        }
                    }
                    else if (currentClaim is not null)
                    {
                        removedClaims.Add(currentClaim);
                    }
                }

                var existingRoleClaims = existingClaims.Where(claim => claim.Type == DiscordClaimTypes.Role).ToDictionary(claim => claim.Value);

                foreach (var updatedRole in guildMember.RoleNames)
                {
                    if (existingRoleClaims.TryGetValue(updatedRole, out var existingRoleClaim))
                    {
                        existingRoleClaims.Remove(updatedRole);
                    }
                    else
                    {
                        newClaims.Add(new Claim(DiscordClaimTypes.Role, updatedRole));
                    }
                }

                foreach (var existingRoleClaim in existingRoleClaims.Values)
                {
                    removedClaims.Add(existingRoleClaim);
                }

                foreach (var newRole in guildMember.RoleNames)
                {
                    if (!existingClaims.Any(claim => claim.Type == DiscordClaimTypes.Role && claim.Value == newRole))
                    {
                        newClaims.Add(new Claim(DiscordClaimTypes.Role, newRole));
                    }
                }

                if (newClaims.Count != 0)
                {
                    identityResult = await _signInManager.UserManager.AddClaimsAsync(user, newClaims);

                    if (!identityResult.Succeeded)
                    {
                        return LocalRedirect("~/loginerror?reason=0");
                    }
                }

                if (removedClaims.Count != 0)
                {
                    identityResult = await _signInManager.UserManager.RemoveClaimsAsync(user, removedClaims);

                    if (!identityResult.Succeeded)
                    {
                        return LocalRedirect("~/loginerror?reason=0");
                    }
                }

                await _signInManager.SignInAsync(user, isPersistent: true, authenticationMethod: info.LoginProvider);

                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity?.Name, info.LoginProvider);

                return LocalRedirect(returnUrl);
            }
            else
            {
                user = new AppUser
                {
                    Id = info.ProviderKey,
                    UserName = guildMember.DisplayName
                };

                identityResult = await _signInManager.UserManager.CreateAsync(user);

                if (identityResult.Succeeded)
                {
                    identityResult = await _signInManager.UserManager.AddLoginAsync(user, info);

                    if (identityResult.Succeeded)
                    {
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var claims = info.Principal.Claims.Where(claim => claim.Type.StartsWith(DiscordClaimTypes.ClaimPrefix)).ToList();

                        foreach (var role in guildMember.RoleNames)
                        {
                            claims.Add(new Claim(DiscordClaimTypes.Role, role));

                            foreach (var (appRole, discordRole) in _roles.AllRoles)
                            {
                                if (string.Equals(role, discordRole, StringComparison.OrdinalIgnoreCase))
                                {
                                    claims.Add(new Claim(AppClaimTypes.Role, appRole));
                                }
                            }
                        }

                        identityResult = await _signInManager.UserManager.AddClaimsAsync(user, claims);

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
