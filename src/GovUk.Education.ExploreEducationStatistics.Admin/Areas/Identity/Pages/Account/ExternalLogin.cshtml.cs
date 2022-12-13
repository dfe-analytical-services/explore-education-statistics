using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private const string LoginErrorMessage = "Sorry, there was a problem logging you in.";

        private readonly ISignInManagerDelegate _signInManager;
        private readonly IUserManagerDelegate _userManager;
        private readonly ContentDbContext _contentDbContext;
        private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            ISignInManagerDelegate signInManager,
            IUserManagerDelegate userManager,
            ILogger<ExternalLoginModel> logger,
            ContentDbContext contentDbContext,
            UsersAndRolesDbContext usersAndRolesDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _contentDbContext = contentDbContext;
            _usersAndRolesDbContext = usersAndRolesDbContext;
        }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
        public string ReturnUrl { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        // ReSharper disable once MemberCanBePrivate.Global
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

        /// <remarks>
        /// Argument names are important - e.g. if `returnUrl`'s name is changed, the returnUrl will always be null
        /// </remarks>
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            var returnUrlOrFallback = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                _logger.LogError("An error was received form the external Identity Provider. " +
                                 "Unable to log user in - {RemoteError}", remoteError);
                return RedirectToLoginPageWithError(returnUrlOrFallback);
            }

            // Get the provided login information from the external Identity Provider (IdP).
            var info = await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                _logger.LogError("Unable to retrieve any login information from external Identity Provider." +
                                 "Unable to log user in");
                return RedirectToLoginPageWithError(returnUrlOrFallback);
            }

            // See if we have previously-recorded login details that match the details we have been provided by the
            // IdP.
            var existingMatchingLoginDetails = await _usersAndRolesDbContext
                .UserLogins
                .AsQueryable()
                .SingleOrDefaultAsync(login => login.ProviderKey == info.ProviderKey
                                               && login.LoginProvider == info.LoginProvider);

            // If we've had a successful login in the past with this set of IdP credentials, log the user in using
            // these credentials.
            if (existingMatchingLoginDetails != null)
            {
                return await HandleLoginWithRecognisedProviderDetails(returnUrlOrFallback, info);
            }

            // Gather some user information from the Claims given to the service from the External IdP.
            // We can use these to see if we have a matching user in the system already, or for creating
            // a new user if this is a user logging in after being invited.
            var userDetails = GetUserDetailsFromClaims(info);

            if (userDetails.email == null)
            {
                _logger.LogError(
                    "No Email Claim was provided by the external Identity Provider in either the " +
                    "{EmailClaimType} or {NameClaimType} claims", ClaimTypes.Email, ClaimTypes.Name);
                return RedirectToLoginPageWithError(returnUrlOrFallback);
            }

            // See if we have an existing AspNetUsers user with this email address already in the system.
            // If we do, this should mean that we're seeing a new set of login information from the IdP for this
            // same user, or potentially a set of login information from a new IdP but for this same user.
            var existingAspNetUser = await _usersAndRolesDbContext
                .Users
                .AsQueryable()
                .SingleOrDefaultAsync(user => user.Email.ToLower() == userDetails.email.ToLower());

            if (existingAspNetUser != null)
            {
                return await HandleLoginExistingUser(returnUrlOrFallback, existingAspNetUser, info);
            }

            // Otherwise, the user does not yet exist in the service, and these login details from the IdP are
            // representing a brand-new user to the service.  In order for a brand-new user to use the service, they
            // need to be invited in with a particular global or resource-specific role.
            var inviteToSystem = await _usersAndRolesDbContext
                .UserInvites
                .IgnoreQueryFilters()
                .Include(i => i.Role)
                .FirstOrDefaultAsync(invite =>
                    invite.Email.ToLower() == userDetails.email.ToLower() && invite.Accepted == false);

            // If the newly logging in User has an unaccepted invite with a matching email address, register them with
            // the Identity Framework and also create any "internal" User records too if they don't already exist.  If
            // they *do* exist already, link the new AspNetUsers user with the existing "internal" User via a
            // one-to-one id mapping.
            if (inviteToSystem != null)
            {
                return await HandleNewInvitedUser(
                    inviteToSystem,
                    userDetails.email,
                    userDetails.firstName,
                    userDetails.lastName,
                    info,
                    returnUrlOrFallback);
            }

            _logger.LogError(
                "No existing user with a matching email address exists in the system, nor is the user invited " +
                "to use the system.  Unable to log user in");
            return RedirectToLoginPageWithError(returnUrlOrFallback);
        }

        private async Task<IActionResult> HandleLoginWithRecognisedProviderDetails(string returnUrl, ExternalLoginInfo info)
        {
            return await LoginUserWithProviderDetails(returnUrl, info);
        }

        private IActionResult RedirectToLoginPageWithError(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ErrorMessage = LoginErrorMessage;
            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }

        private async Task<IActionResult> HandleLoginExistingUser(
            string returnUrl,
            ApplicationUser existingAspNetUser,
            ExternalLoginInfo info)
        {
            // If this user already exists in the AspNetUsers table, then we must already had at least one
            // successful login for this user in the past from a trusted IdP, so we should find at least one entry
            // in the AspNetUserLogins table where we recorded the user's old IdP Provider details in the past.
            var existingLoginTokens = await _userManager.GetLoginsAsync(existingAspNetUser);

            if (existingLoginTokens.IsNullOrEmpty())
            {
                _logger.LogError("Unable to find previous login information for existing AspNetUser {UserId} " +
                                   "- this should not be possible - unable to log user in", existingAspNetUser.Id);
                return RedirectToLoginPageWithError(returnUrl);
            }

            var recordIdpLoginDetailsResult = await _userManager.AddLoginAsync(existingAspNetUser, info);

            // If for whatever reason we can't create the latest set of login details, we can't log the user
            // in.
            if (!recordIdpLoginDetailsResult.Succeeded)
            {
                return HandleAddingLoginIdentityFailure(returnUrl, existingAspNetUser, recordIdpLoginDetailsResult);
            }

            // Now that we definitely have login details for the existing user, log them in.
            return await LoginUserWithProviderDetails(returnUrl, info);
        }

        private async Task<IActionResult> LoginUserWithProviderDetails(
            string returnUrl,
            ExternalLoginInfo info)
        {
            var loginResult = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            if (loginResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            if (loginResult.IsLockedOut)
            {
                _logger.LogError("User is locked out - unable to log in");

                ErrorMessage = LoginErrorMessage;

                return RedirectToPage("./Lockout");
            }

            return RedirectToLoginPageWithError(returnUrl);
        }

        private async Task<IActionResult> HandleNewInvitedUser(
            UserInvite inviteToSystem,
            string email,
            string firstName,
            string lastName,
            ExternalLoginInfo info,
            string returnUrl)
        {
            if (inviteToSystem.Expired)
            {
                return await HandleExpiredInvite(inviteToSystem, email);
            }
            
            // Mark the invite as accepted.
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
            var newAspNetUser = new ApplicationUser
            {
                Id = existingInternalUser?.Id.ToString() ?? Guid.NewGuid().ToString(),
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName
            };

            var createdIdentityUserResult = await _userManager.CreateAsync(newAspNetUser);

            if (!createdIdentityUserResult.Succeeded)
            {
                return HandleCreateIdentityFailure(returnUrl, createdIdentityUserResult);
            }

            var addedIdentityUserRoles = await _userManager.AddToRoleAsync(newAspNetUser, inviteToSystem.Role.Name);

            if (!addedIdentityUserRoles.Succeeded)
            {
                _logger.LogError("Error adding role to invited User - unable to log in");

                return HandleIdentityResultFailure(returnUrl, addedIdentityUserRoles);
            }

            var recordIdpLoginDetailsResult = await _userManager.AddLoginAsync(newAspNetUser, info);

            if (!recordIdpLoginDetailsResult.Succeeded)
            {
                return HandleAddingLoginIdentityFailure(returnUrl, newAspNetUser, recordIdpLoginDetailsResult);
            }

            // If adding the new Identity Framework user records succeeded, continue on to create internal User
            // and Role records for the application itself and sign the user in.

            // If we didn't yet have an existing "internal" User record matching this new login in the Users
            // table, create one now, being sure to establish the one-to-one id relationship between the
            // AspNetUsers record and the Users record.
            if (existingInternalUser == null)
            {
                var newInternalUser = new User
                {
                    Id = Guid.Parse(newAspNetUser.Id),
                    FirstName = newAspNetUser.FirstName,
                    LastName = newAspNetUser.LastName,
                    Email = newAspNetUser.Email
                };

                await _contentDbContext.Users.AddAsync(newInternalUser);

                var releaseRolesToCreate = await 
                    GetUserReleaseInvites(email)
                    .Select(invite => new UserReleaseRole
                    {
                        ReleaseId = invite.ReleaseId,
                        Role = invite.Role,
                        UserId = newInternalUser.Id,
                        Created = DateTime.UtcNow,
                        CreatedById = invite.CreatedById,
                    })
                    .ToListAsync();
                await _contentDbContext.UserReleaseRoles.AddRangeAsync(releaseRolesToCreate);

                var publicationRolesToCreate = await 
                    GetUserPublicationInvites(email)
                    .Select(invite => new UserPublicationRole
                    {
                        PublicationId = invite.PublicationId,
                        Role = invite.Role,
                        UserId = newInternalUser.Id,
                        Created = DateTime.UtcNow,
                        CreatedById = invite.CreatedById,
                    })
                    .ToListAsync();
                await _contentDbContext.AddRangeAsync(publicationRolesToCreate);
            }

            await _contentDbContext.SaveChangesAsync();
            await _usersAndRolesDbContext.SaveChangesAsync();

            await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider,
                info.ProviderKey,
                isPersistent: false,
                bypassTwoFactor: true);

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            // This method should no longer be used but just in case, return user to login page if it is hit.
            var pageRedirect = RedirectToPage("./Login", new {ReturnUrl = returnUrl ?? Url.Content("~/")});
            return await Task.FromResult(pageRedirect);
        }

        private async Task<IActionResult> HandleExpiredInvite(UserInvite inviteToSystem, string email)
        {
            var releaseInvites = GetUserReleaseInvites(email);
            var publicationInvites = GetUserPublicationInvites(email);
            _usersAndRolesDbContext.UserInvites.Remove(inviteToSystem);
            _contentDbContext.UserReleaseInvites.RemoveRange(releaseInvites);
            _contentDbContext.UserPublicationInvites.RemoveRange(publicationInvites);
            await _usersAndRolesDbContext.SaveChangesAsync();
            await _contentDbContext.SaveChangesAsync();
            return RedirectToPage("./InviteExpired");
        }

        private IQueryable<UserPublicationInvite> GetUserPublicationInvites(string email)
        {
            return _contentDbContext
                .UserPublicationInvites
                .Where(invite => invite.Email.ToLower().Equals(email.ToLower()));
        }

        private IQueryable<UserReleaseInvite> GetUserReleaseInvites(string email)
        {
            return _contentDbContext
                .UserReleaseInvites
                .Where(invite => invite.Email.ToLower().Equals(email.ToLower()));
        }

        private IActionResult HandleCreateIdentityFailure(string returnUrl, IdentityResult createdIdentityUserResult)
        {
            _logger.LogError("Unable to create AspNetUsers record for invited User - unable to log in");

            return HandleIdentityResultFailure(returnUrl, createdIdentityUserResult);
        }

        private (
            string email,
            string firstName,
            string lastName) GetUserDetailsFromClaims(ExternalLoginInfo info)
        {
            var email = info.Principal.FindFirstValue(ClaimTypes.Email) != null
                ? info.Principal.FindFirstValue(ClaimTypes.Email)
                : info.Principal.FindFirstValue(ClaimTypes.Name);

            // Try to infer the user's name from the explicit "Given Name" and "Surname" Claims
            // if they are available.
            var givenName = info.Principal.FindFirstValue(ClaimTypes.GivenName);
            var surname = info.Principal.FindFirstValue(ClaimTypes.Surname);

            if (givenName != null && surname != null) {
                return (email, givenName, surname);
            }

            // Failing finding explicit "Given Name" and "Surname" Claims on which to base
            // the user's name, next use the more generic "Name" Claim if it's available.
            var nameClaim = info.Principal.FindFirstValue(JwtClaimTypes.Name);

            if (nameClaim == null)
            {
                _logger.LogWarning(
                    @"No Name Claims found during user login, so no first or last name 
                    information is available from {GivenNameClaimType}, {SurnameClaimType} or 
                    {NameJwtClaimType} - falling back to blank first and last names",
                    ClaimTypes.GivenName, ClaimTypes.Surname, JwtClaimTypes.Name);
                return (email, "", "");
            }

            var nameClaimParts = nameClaim.Trim().Split(' ');

            if (nameClaimParts.Length > 1) {
                return (email, nameClaimParts.First(), nameClaimParts.Last());
            }

            return (email, nameClaim, "");
        }

        private IActionResult HandleAddingLoginIdentityFailure(
            string returnUrl,
            ApplicationUser user,
            IdentityResult identityResult)
        {
            _logger.LogError("Unable to create the latest set of login details for " +
                             "user {UserId} - unable to log user in", user.Id);

            return HandleIdentityResultFailure(returnUrl, identityResult);
        }

        private IActionResult HandleIdentityResultFailure(string returnUrl, IdentityResult identityResult)
        {
            identityResult.Errors.ForEach(error =>
                ModelState.AddModelError(string.Empty, error.Description));

            ErrorMessage = LoginErrorMessage;

            return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
        }
    }

    public interface ISignInManagerDelegate
    {
        AuthenticationProperties ConfigureExternalAuthenticationProperties(
            string provider,
            string redirectUrl,
            string userId = null);

        Task<ExternalLoginInfo> GetExternalLoginInfoAsync(
            string expectedXsrf = null);

        Task<SignInResult> ExternalLoginSignInAsync(
            string loginProvider,
            string providerKey,
            bool isPersistent,
            bool bypassTwoFactor);
    }

    public class SignInManagerDelegate : ISignInManagerDelegate
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SignInManagerDelegate(SignInManager<ApplicationUser> signInManager)
        {
            _signInManager = signInManager;
        }

        public AuthenticationProperties ConfigureExternalAuthenticationProperties(string provider, string redirectUrl,
            string userId = null)
        {
            return _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, userId);
        }

        public Task<ExternalLoginInfo> GetExternalLoginInfoAsync(string expectedXsrf = null)
        {
            return _signInManager.GetExternalLoginInfoAsync(expectedXsrf);
        }

        public Task<SignInResult> ExternalLoginSignInAsync(string loginProvider, string providerKey, bool isPersistent, bool bypassTwoFactor)
        {
            return _signInManager.ExternalLoginSignInAsync(loginProvider, providerKey, isPersistent, bypassTwoFactor);
        }
    }

    public interface IUserManagerDelegate
    {
        Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user);

        Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login);

        Task<IdentityResult> CreateAsync(ApplicationUser user);

        Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role);
    }

    public class UserManagerDelegate : IUserManagerDelegate
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserManagerDelegate(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user)
        {
            return _userManager.GetLoginsAsync(user);
        }

        public Task<IdentityResult> AddLoginAsync(ApplicationUser user, UserLoginInfo login)
        {
            return _userManager.AddLoginAsync(user, login);
        }

        public Task<IdentityResult> CreateAsync(ApplicationUser user)
        {
            return _userManager.CreateAsync(user);
        }

        public Task<IdentityResult> AddToRoleAsync(ApplicationUser user, string role)
        {
            return _userManager.AddToRoleAsync(user, role);
        }
    }
}
