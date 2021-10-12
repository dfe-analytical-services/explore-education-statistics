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
                _logger.LogInformation("{0} logged in with {1} provider",
                    info.Principal.Identity.Name,
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

                if (nameClaim == null)
                {
                    _logger.LogWarning($@"No Name Claims found during user login, so no first or last name 
                                       information is available from {ClaimTypes.GivenName}, {ClaimTypes.Surname} or 
                                       {JwtClaimTypes.Name} - falling back to blank first and last names");
                    firstName = "";
                    lastName = "";
                }
                else
                {
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
            }

            if (email == null)
            {
                _logger.LogWarning($@"No Email Claim found during user login from {ClaimTypes.Email} or 
                                           {ClaimTypes.Name}");
                ErrorMessage = "Email address not returned from Identity Provider";
                return RedirectToPage("./Login", new {ReturnUrl = returnUrl});
            }

            // Check if the user is invited
            var inviteToSystem = await _usersAndRolesDbContext
                .UserInvites
                .Include(i => i.Role)
                .FirstOrDefaultAsync(i =>
                    i.Email.ToLower() == email.ToLower()
                    && i.Accepted == false);

            // If the newly logging in User has an unaccepted invite with a matching email address to the User logging
            // in, register them with the Identity Framework and also create any "internal" User records too if they
            // don't already exist.  If they *do* exist already, link the new Identity Framework user with the existing
            // "internal" User via the same id (more below).
            if (inviteToSystem != null)
            {
                // Mark the invite as accepted
                inviteToSystem.Accepted = true;
                _usersAndRolesDbContext.UserInvites.Update(inviteToSystem);

                // See if we have an "internal" User record in existence yet that has a matching email address to the
                // new user logging in.  If we do, create this new AspNetUser record with a matching one-to-one id.
                // Otherwise, later on we'll create a new "internal" Users record with an id matching this AspNetUser's
                // id, continuing to establish the one-to-one relationship.
                var existingInternalUser = await _contentDbContext
                    .Users
                    .AsQueryable()
                    .SingleOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

                // Create a new set of AspNetUser records for the Identity Framework.
                var identityUser = new ApplicationUser
                {
                    Id = existingInternalUser?.Id.ToString() ?? Guid.NewGuid().ToString(),
                    UserName = email,
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName
                };

                var createdIdentityUserResult = await _userManager.CreateAsync(identityUser);
                var addedIdentityUserRoles = await _userManager.AddToRoleAsync(identityUser, inviteToSystem.Role.Name);
                var recordIdpLoginDetailsResult = await _userManager.AddLoginAsync(identityUser, info);

                // If adding the new Identity Framework user records succeeded, continue on to create internal User
                // and Role records for the application itself and sign the user in.
                if (createdIdentityUserResult.Succeeded && addedIdentityUserRoles.Succeeded && recordIdpLoginDetailsResult.Succeeded)
                {
                    // If we didn't yet have an existing "internal" User record matching this new login in the Users
                    // table, create one now, being sure to establish the one-to-one id relationship between the
                    // AspNetUsers record and the Users record.
                    if (existingInternalUser == null)
                    {
                        var newInternalUser = new User
                        {
                            Id = Guid.Parse(identityUser.Id),
                            FirstName = identityUser.FirstName,
                            LastName = identityUser.LastName,
                            Email = identityUser.Email
                        };

                        await _contentDbContext.Users.AddAsync(newInternalUser);

                        var releaseInvites = _contentDbContext
                            .UserReleaseInvites
                            .AsQueryable()
                            .Where(i => i.Email.ToLower() == identityUser.Email.ToLower());

                        await releaseInvites.ForEachAsync(invite =>
                        {
                            _contentDbContext.AddAsync(new UserReleaseRole
                            {
                                ReleaseId = invite.ReleaseId,
                                Role = invite.Role,
                                UserId = newInternalUser.Id,
                            });

                            invite.Accepted = true;
                        });
                    }

                    await _contentDbContext.SaveChangesAsync();
                    await _usersAndRolesDbContext.SaveChangesAsync();

                    await _signInManager.SignInAsync(identityUser, isPersistent: false);
                    _logger.LogInformation("User created an account using {0} provider", info.LoginProvider);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in createdIdentityUserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                foreach (var error in addedIdentityUserRoles.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                foreach (var error in recordIdpLoginDetailsResult.Errors)
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
