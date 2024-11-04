#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class SignInService : ISignInService
{
    private readonly ILogger<SignInService> _logger;
    private readonly IUserService _userService;
    private readonly UsersAndRolesDbContext _usersAndRolesDbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ContentDbContext _contentDbContext;
    private readonly IUserReleaseRoleRepository _releaseRoleRepository;
    private readonly IUserPublicationRoleRepository _publicationRoleRepository;
    private readonly IUserReleaseInviteRepository _releaseInviteRepository;
    private readonly IUserPublicationInviteRepository _publicationInviteRepository;

    public SignInService(
        ILogger<SignInService> logger,
        IUserService userService,
        UsersAndRolesDbContext usersAndRolesDbContext,
        UserManager<ApplicationUser> userManager,
        ContentDbContext contentDbContext,
        IUserReleaseRoleRepository releaseRoleRepository,
        IUserPublicationRoleRepository publicationRoleRepository,
        IUserReleaseInviteRepository releaseInviteRepository,
        IUserPublicationInviteRepository publicationInviteRepository)
    {
        _logger = logger;
        _userService = userService;
        _usersAndRolesDbContext = usersAndRolesDbContext;
        _userManager = userManager;
        _contentDbContext = contentDbContext;
        _releaseRoleRepository = releaseRoleRepository;
        _publicationRoleRepository = publicationRoleRepository;
        _releaseInviteRepository = releaseInviteRepository;
        _publicationInviteRepository = publicationInviteRepository;
    }

    public async Task<Either<ActionResult, SignInResponseViewModel>> RegisterOrSignIn()
    {
        // Get the profile of the current user who has logged into the external Identity Provider.
        var profile = _userService.GetProfileFromClaims();

        // If this email address is recognised as belonging to an existing user in the service, they have
        // been registered previously and can continue to use the service.
        var existingUser = await _userManager.FindByEmailAsync(profile.Email);

        if (existingUser != null)
        {
            return new SignInResponseViewModel(
                LoginResult: LoginResult.LoginSuccess,
                UserProfile: new UserProfile(
                    Id: Guid.Parse(existingUser.Id),
                    FirstName: existingUser.FirstName));
        }

        // If the email address does not match an existing user in the service, see if they have been invited.
        var inviteToSystem = await _usersAndRolesDbContext
            .UserInvites
            .IgnoreQueryFilters() // Retrieve expired invites as well as active ones.
            .Include(i => i.Role)
            .FirstOrDefaultAsync(invite =>
                invite.Email.ToLower() == profile.Email.ToLower() && invite.Accepted == false);

        // If the newly logging in User has an unaccepted invite with a matching email address, register them with
        // the Identity Framework and also create any "internal" User records too if they don't already exist.  If
        // they *do* exist already, link the new AspNetUsers user with the existing "internal" User via a
        // one-to-one id mapping.
        if (inviteToSystem != null)
        {
            return await HandleNewInvitedUser(inviteToSystem, profile);
        }

        return new SignInResponseViewModel(LoginResult.NoInvite);
    }

    private async Task<Either<ActionResult, SignInResponseViewModel>> HandleNewInvitedUser(
        UserInvite inviteToSystem,
        UserProfileFromClaims profile)
    {
        if (inviteToSystem.Expired)
        {
            await HandleExpiredInvite(inviteToSystem, profile.Email);
            return new SignInResponseViewModel(LoginResult.ExpiredInvite);
        }

        inviteToSystem.Accepted = true;

        // This will also fetch soft-deleted users
        var existingInternalUser = await _contentDbContext
            .Users
            .AsQueryable()
            .SingleOrDefaultAsync(u => u.Email.ToLower() == profile.Email.ToLower());

        // Create a new set of AspNetUser records for the Identity Framework.
        var newAspNetUser = new ApplicationUser
        {
            Id = existingInternalUser?.Id.ToString() ?? Guid.NewGuid().ToString(),
            UserName = profile.Email,
            Email = profile.Email, // do these need lower-casing?
            FirstName = profile.FirstName,
            LastName = profile.LastName
        };

        await _usersAndRolesDbContext.Users.AddAsync(newAspNetUser);
        await _usersAndRolesDbContext.SaveChangesAsync();

        // Add them to their global role.
        var addedIdentityUserRoles = await _userManager.AddToRoleAsync(newAspNetUser, inviteToSystem.Role.Name);

        if (!addedIdentityUserRoles.Succeeded)
        {
            _logger.LogError("Error adding role to invited User - unable to log in");
            return new StatusCodeResult(500);
        }

        // Now we have created Identity Framework user records, we can create internal User and Role records
        // for the application itself.

        // If the user was previously soft deleted, we undo it. When a user is soft deleted, all the user's database
        // entries are removed *except* for the ContentDb's User table entry.
        if (existingInternalUser?.SoftDeleted != null)
        {
            existingInternalUser.FirstName = newAspNetUser.FirstName;
            existingInternalUser.LastName = newAspNetUser.LastName;
            existingInternalUser.SoftDeleted = null;
            await HandleReleaseInvites(existingInternalUser.Id, existingInternalUser.Email);
            await HandlePublicationInvites(existingInternalUser.Id, existingInternalUser.Email);
        }

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
            await HandleReleaseInvites(newInternalUser.Id, newInternalUser.Email);
            await HandlePublicationInvites(newInternalUser.Id, newInternalUser.Email);
        }

        await _contentDbContext.SaveChangesAsync();
        await _usersAndRolesDbContext.SaveChangesAsync();

        return new SignInResponseViewModel(
            LoginResult: LoginResult.RegistrationSuccess,
            UserProfile: new UserProfile(
                Id: Guid.Parse(newAspNetUser.Id),
                FirstName: newAspNetUser.FirstName));
    }

    private async Task HandleReleaseInvites(Guid newUserId, string email)
    {
        var releaseInvites = await _releaseInviteRepository.ListByEmail(email);
        await releaseInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => _releaseRoleRepository.Create(
                userId: newUserId,
                releaseVersionId: invite.ReleaseVersionId,
                role: invite.Role,
                createdById: invite.CreatedById));
    }

    private async Task HandlePublicationInvites(Guid newUserId, string email)
    {
        var publicationInvites = await _publicationInviteRepository.ListByEmail(email);
        await publicationInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => _publicationRoleRepository.Create(
                userId: newUserId,
                publicationId: invite.PublicationId,
                role: invite.Role,
                createdById: invite.CreatedById));
    }

    private async Task HandleExpiredInvite(
        UserInvite inviteToSystem,
        string email)
    {
        var releaseInvites = await _releaseInviteRepository.ListByEmail(email);
        var publicationInvites = await _publicationInviteRepository.ListByEmail(email);
        _usersAndRolesDbContext.UserInvites.Remove(inviteToSystem);
        _contentDbContext.UserReleaseInvites.RemoveRange(releaseInvites);
        _contentDbContext.UserPublicationInvites.RemoveRange(publicationInvites);
        await _usersAndRolesDbContext.SaveChangesAsync();
        await _contentDbContext.SaveChangesAsync();
    }
}
