#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Database;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class SignInService(
    ILogger<SignInService> logger,
    IUserService userService,
    UsersAndRolesDbContext usersAndRolesDbContext,
    UserManager<ApplicationUser> userManager,
    ContentDbContext contentDbContext,
    IUserReleaseRoleAndInviteManager userReleaseRoleAndInviteManager,
    IUserPublicationRoleAndInviteManager userPublicationRoleAndInviteManager,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserPublicationInviteRepository userPublicationInviteRepository) : ISignInService
{
    public async Task<Either<ActionResult, SignInResponseViewModel>> RegisterOrSignIn()
    {
        // Get the profile of the current user who has logged into the external Identity Provider.
        var profile = userService.GetProfileFromClaims();

        // If this email address is recognised as belonging to an existing user in the service, they have
        // been registered previously and can continue to use the service.
        var existingUser = await userManager.FindByEmailAsync(profile.Email);

        if (existingUser != null)
        {
            return new SignInResponseViewModel(
                LoginResult: LoginResult.LoginSuccess,
                UserProfile: new UserProfile(
                    Id: Guid.Parse(existingUser.Id),
                    FirstName: existingUser.FirstName));
        }

        // If the email address does not match an existing user in the service, see if they have been invited.
        var inviteToSystem = await usersAndRolesDbContext
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
        var existingInternalUser = await contentDbContext
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

        await usersAndRolesDbContext.Users.AddAsync(newAspNetUser);
        await usersAndRolesDbContext.SaveChangesAsync();

        // Add them to their global role.
        var addedIdentityUserRoles = await userManager.AddToRoleAsync(newAspNetUser, inviteToSystem.Role.Name);

        if (!addedIdentityUserRoles.Succeeded)
        {
            logger.LogError("Error adding role to invited User - unable to log in");
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

            await contentDbContext.Users.AddAsync(newInternalUser);
            await HandleReleaseInvites(newInternalUser.Id, newInternalUser.Email);
            await HandlePublicationInvites(newInternalUser.Id, newInternalUser.Email);
        }

        await contentDbContext.SaveChangesAsync();
        await usersAndRolesDbContext.SaveChangesAsync();

        return new SignInResponseViewModel(
            LoginResult: LoginResult.RegistrationSuccess,
            UserProfile: new UserProfile(
                Id: Guid.Parse(newAspNetUser.Id),
                FirstName: newAspNetUser.FirstName));
    }

    private async Task HandleReleaseInvites(Guid newUserId, string email)
    {
        var releaseInvites = await userReleaseInviteRepository.ListByEmail(email);
        await releaseInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => userReleaseRoleAndInviteManager.Create(
                userId: newUserId,
                releaseVersionId: invite.ReleaseVersionId,
                role: invite.Role,
                createdById: invite.CreatedById));
    }

    private async Task HandlePublicationInvites(Guid newUserId, string email)
    {
        var publicationInvites = await userPublicationInviteRepository.ListByEmail(email);
        await publicationInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => userPublicationRoleAndInviteManager.Create(
                userId: newUserId,
                publicationId: invite.PublicationId,
                role: invite.Role,
                createdById: invite.CreatedById));
    }

    private async Task HandleExpiredInvite(
        UserInvite inviteToSystem,
        string email)
    {
        await contentDbContext.RequireTransaction(async () =>
        {
            await userReleaseInviteRepository.RemoveByUser(email);
            await userPublicationInviteRepository.RemoveByUser(email);

            usersAndRolesDbContext.UserInvites.Remove(inviteToSystem);
            await usersAndRolesDbContext.SaveChangesAsync();
        });
    }
}
