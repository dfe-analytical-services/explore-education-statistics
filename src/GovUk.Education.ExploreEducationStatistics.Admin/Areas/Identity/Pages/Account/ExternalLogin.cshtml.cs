using System;
using System.Linq;
using System.Security.Claims;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static System.StringComparison;

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

        public string ReturnUrl { get; set; }

        [TempData] public string ErrorMessage { get; set; }


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
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) != null 
                ? info.Principal.FindFirstValue(ClaimTypes.Email)
                : info.Principal.FindFirstValue(ClaimTypes.Name);

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
            var inviteToSystem = await _usersAndRolesDbContext
                .UserInvites
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i =>
                    i.Email.ToLower() == email.ToLower()
                    && i.Accepted == false);

            // If the user has an invite register them automatically
            if (inviteToSystem != null)
            {
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };
                
                var createdUserResult = await _userManager.CreateAsync(user);

                var addedUserRoles = await _userManager.AddToRoleAsync(user, inviteToSystem.Role.Name);

                if (createdUserResult.Succeeded && addedUserRoles.Succeeded)
                {
                    var newUser = new User
                    {
                        Id = new Guid(user.Id),
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email
                    };
                    
                    _contentDbContext.Users.Add(newUser);

                    await _contentDbContext.SaveChangesAsync();

                    createdUserResult = await _userManager.AddLoginAsync(user, info);
                    if (createdUserResult.Succeeded)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                        inviteToSystem.Accepted = true;
                        _usersAndRolesDbContext.UserInvites.Update(inviteToSystem);
                        await _usersAndRolesDbContext.SaveChangesAsync();

                        var releaseInvites = _contentDbContext
                            .UserReleaseInvites
                            .Where(i => string.Equals(i.Email, user.Email, CurrentCultureIgnoreCase));

                        await releaseInvites.ForEachAsync(i =>
                            
                            _contentDbContext.Add(new UserReleaseRole
                            {
                                ReleaseId = i.ReleaseId,
                                Role = i.Role,
                                UserId = newUser.Id,
                            })
                        );
                        
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in createdUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                foreach (var error in addedUserRoles.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ReturnUrl = returnUrl;

            return Page();
        }


        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            // This method should no longer be used but just incase return user to login page if its hit
            return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
        }
    }
}