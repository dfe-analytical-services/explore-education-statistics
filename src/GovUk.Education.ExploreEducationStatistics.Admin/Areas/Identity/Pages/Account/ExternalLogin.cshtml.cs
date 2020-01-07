using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ContentDbContext _contentDbContext;
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            ContentDbContext contentDbContext,
            UsersAndRolesDbContext usersAndRolesDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _contentDbContext = contentDbContext;
            _usersAndRolesDbContext = usersAndRolesDbContext;
        }

        [BindProperty] public InputModel Input { get; set; }

        public string LoginProvider { get; set; }

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required] [EmailAddress] public string Email { get; set; }

            [Required] public string FirstName { get; set; }

            [Required] public string LastName { get; set; }
        }

        public IActionResult OnGetAsync()
        {
            return RedirectToPage("./Login");
        }

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new {returnUrl});
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
                isPersistent: false, bypassTwoFactor: true);
            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name,
                    info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            var firstName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var lastName = info.Principal.FindFirstValue(ClaimTypes.Surname);
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);

            // Ensure names are set
            if (firstName == null && lastName == null)
            {
                var nameClaim = info.Principal.FindFirstValue(JwtClaimTypes.Name);

                if (nameClaim.IndexOf(' ') > 0)
                {
                    firstName = nameClaim.Substring(0, nameClaim.IndexOf(' '));
                    lastName = nameClaim.Substring(nameClaim.IndexOf(' ') + 1);
                }
                else
                {
                    firstName = nameClaim;
                    lastName = "";
                }
            }

            if (email == null)
            {
                ErrorMessage = "Email address not returned from Azure Active Directory";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            // Check if the user is invited
            var invite = await _usersAndRolesDbContext.UserInvites
                .FirstOrDefaultAsync(i =>
                    string.Equals(i.Email, email, StringComparison.CurrentCultureIgnoreCase)
                    && i.Accepted == false);

            // If the user has a invite register them automatically
            if (invite != null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                var createdUserResult = await _userManager.CreateAsync(user);
                
                if (createdUserResult.Succeeded)
                {
                    _contentDbContext.Users.Add(new User
                    {
                        Id = new Guid(user.Id),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    });

                    await _contentDbContext.SaveChangesAsync();

                    createdUserResult = await _userManager.AddLoginAsync(user, info);
                    if (createdUserResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        var userId = await _userManager.GetUserIdAsync(user);
                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                        var callbackUrl = Url.Page(
                            "/Account/ConfirmEmail",
                            pageHandler: null,
                            values: new {area = "Identity", userId = userId, code = code},
                            protocol: Request.Scheme);

                        await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                            $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                        return LocalRedirect(returnUrl);
                    }
                }
                
                foreach (var error in createdUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }


            // TODO: Show error page rather than registration (leaving this default as fallback)
            ReturnUrl = returnUrl;
            LoginProvider = info.LoginProvider;

            Input = new InputModel
            {
                Email = info.Principal.FindFirstValue(ClaimTypes.Email),
                FirstName = firstName,
                LastName = lastName
            };
            return Page();
        }


        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // This method should no longer be used, return user to login page if its hit
            return RedirectToPage("./Login", new {ReturnUrl = returnUrl});

        }
    }
}