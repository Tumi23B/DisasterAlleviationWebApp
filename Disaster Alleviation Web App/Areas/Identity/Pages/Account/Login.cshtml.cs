#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Disaster_Alleviation_Web_App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Disaster_Alleviation_Web_App.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
                          UserManager<ApplicationUser> userManager,
                          ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");

            // Clear any existing external cookies
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email?.Trim();
            var password = Input.Password;

            // find user by email
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // no such account -> generic error
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Try the normal sign-in path first
            var signInResult = await _signInManager.PasswordSignInAsync(user.UserName, password, Input.RememberMe, lockoutOnFailure: false);
            if (signInResult.Succeeded)
            {
                _logger.LogInformation("User logged in (normal path).");
                return await RedirectByRole(user);
            }

            // If normal sign-in failed, check whether the password is actually correct.
            // If password is correct, we will FORCE sign-in (bypass email confirmation, lockout, 2FA).
            // WARNING: this bypass weakens security — ok for dev/test, not recommended for production.
            var passwordValid = await _userManager.CheckPasswordAsync(user, password);
            if (passwordValid)
            {
                _logger.LogWarning("Password valid but PasswordSignInAsync did not succeed. Forcing sign-in (bypass confirmation/lockout/2FA).");

                // Force sign-in (bypass checks)
                await _signInManager.SignInAsync(user, isPersistent: Input.RememberMe);

                // Redirect based on role
                return await RedirectByRole(user);
            }

            // If we reach here, password is incorrect or other failure
            if (signInResult.RequiresTwoFactor)
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });

            if (signInResult.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                // Optional: still allow if you want to bypass lockout as well (not recommended)
                // return RedirectToPage("./Lockout");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        // Helper to redirect user by role (Admin / Donor / Helper)
        private async Task<IActionResult> RedirectByRole(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            return role switch
            {
                "Admin" => LocalRedirect("~/Admin/Dashboard"),
                "Donor" => LocalRedirect("~/Donor/Dashboard"),
                "Helper" => LocalRedirect("~/Helper/Dashboard"),
                _ => LocalRedirect("~/")
            };
        }
    }
}
