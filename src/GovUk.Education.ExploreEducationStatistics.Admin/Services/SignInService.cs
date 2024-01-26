#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.Areas.Identity.Data.Models;
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

    public async Task<Either<ActionResult, LoginResult>> RegisterOrSignIn()
    {
        // Get the profile of the current user who has logged into the external Identity Provider.
        var profile = _userService.GetProfile();

        // If this email address is recognised as belonging to an existing user in the service, they have
        // been registered previously and can continue to use the service.
        var existingUser = await _userManager.FindByEmailAsync(profile.Email);

        if (existingUser != null)
        {
            // TODO EES-4814 - wanna add any checking for lockout here?
            // TODO EES-4814 - wanna do any last logged in tracking here? Does Identity Framework's services help here?
            return LoginResult.LoginSuccess;
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

        return LoginResult.NoInvite;
    }

    private async Task<Either<ActionResult, LoginResult>> HandleNewInvitedUser(
        UserInvite inviteToSystem,
        UserProfile profile)
    {
        if (inviteToSystem.Expired)
        {
            await HandleExpiredInvite(inviteToSystem, profile.Email);
            return LoginResult.ExpiredInvite;
        }

        // Mark the invite as accepted.
        inviteToSystem.Accepted = true;

        // See if we have an "internal" User record in existence yet that has a matching email address to the
        // new user logging in.  If we do, create this new AspNetUser record with a matching one-to-one id.
        // Otherwise, later on we'll create a new "internal" Users record with an id matching this AspNetUser's
        // id, continuing to establish the one-to-one relationship.
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

        // Add them to their global role.
        var addedIdentityUserRoles = await _userManager.AddToRoleAsync(newAspNetUser, inviteToSystem.Role.Name);

        if (!addedIdentityUserRoles.Succeeded)
        {
            _logger.LogError("Error adding role to invited User - unable to log in");

            // TODO EES-4814 - better result than this
            return new StatusCodeResult(500);
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

            // Accept any invites to specific Releases.
            var releaseInvites = await _releaseInviteRepository.ListByEmail(profile.Email);
            await releaseInvites
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(invite => _releaseRoleRepository.Create(
                    userId: newInternalUser.Id,
                    releaseId: invite.ReleaseId,
                    role: invite.Role,
                    createdById: invite.CreatedById));

            // Accept any invites to specific Publications.
            var publicationInvites = await _publicationInviteRepository.ListByEmail(profile.Email);
            await publicationInvites
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(invite => _publicationRoleRepository.Create(
                    userId: newInternalUser.Id,
                    publicationId: invite.PublicationId,
                    role: invite.Role,
                    createdById: invite.CreatedById));
        }

        await _contentDbContext.SaveChangesAsync();
        await _usersAndRolesDbContext.SaveChangesAsync();

        return LoginResult.RegistrationSuccess;
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
