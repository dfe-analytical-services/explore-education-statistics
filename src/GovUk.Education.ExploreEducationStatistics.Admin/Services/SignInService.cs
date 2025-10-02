#nullable enable
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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class SignInService(
    ILogger<SignInService> logger,
    IUserService userService,
    UsersAndRolesDbContext usersAndRolesDbContext,
    UserManager<ApplicationUser> userManager,
    ContentDbContext contentDbContext,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    IUserReleaseInviteRepository userReleaseInviteRepository,
    IUserPublicationInviteRepository userPublicationInviteRepository
) : ISignInService
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
                UserProfile: new UserProfile(Id: Guid.Parse(existingUser.Id), FirstName: existingUser.FirstName)
            );
        }

        // If the email address does not match an existing user in the service, see if they have been invited.
        var userInvitedToSystem = await contentDbContext
            .Users
            .Include(i => i.Role)
            .Where(u => u.IsPendingInvite)
            .Where(u => u.Email.ToLower() == profile.Email.ToLower())
            .SingleOrDefaultAsync();

        // If the newly logging in User has an unaccepted invite with a matching email address, register them with
        // the Identity Framework.
        // If the unaccepted invite has expired, soft-delete the 'internal' user.
        if (userInvitedToSystem != null)
        {
            return await HandleNewInvitedUser(userInvitedToSystem, profile);
        }

        return new SignInResponseViewModel(LoginResult.NoInvite);
    }

    private async Task<Either<ActionResult, SignInResponseViewModel>> HandleNewInvitedUser(
        User userInvitedToSystem,
        UserProfileFromClaims profile
    )
    {
        if (userInvitedToSystem.ShouldBeExpired)
        {
            await HandleExpiredInvite(userInvitedToSystem, profile.Email);
            return new SignInResponseViewModel(LoginResult.ExpiredInvite);
        }

        // Create Identity user
        var newAspNetUser = new ApplicationUser
        {
            Id = userInvitedToSystem.Id.ToString(),
            UserName = profile.Email,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
        };

        await contentDbContext.RequireTransaction(async () =>
        {
            usersAndRolesDbContext.Users.Add(newAspNetUser);

            // Add them to their global role.
            var roleResult = await userManager.AddToRoleAsync(newAspNetUser, userInvitedToSystem.Role.Name!);

            if (!roleResult.Succeeded)
            {
                logger.LogError("Error adding role to invited User - unable to log in");
                throw new InvalidOperationException("Failed to add role to invited user");
            }

            // Now we have created Identity Framework user records, we can create internal Role
            // for the application itself.

            // Update your domain user
            userInvitedToSystem.FirstName = newAspNetUser.FirstName;
            userInvitedToSystem.LastName = newAspNetUser.LastName;
            userInvitedToSystem.Active = true;

            await HandleReleaseInvites(userInvitedToSystem.Id, userInvitedToSystem.Email);
            await HandlePublicationInvites(userInvitedToSystem.Id, userInvitedToSystem.Email);

            await usersAndRolesDbContext.SaveChangesAsync();
            await contentDbContext.SaveChangesAsync();
        });

        return new SignInResponseViewModel(
            LoginResult.RegistrationSuccess,
            new UserProfile(
                Guid.Parse(newAspNetUser.Id),
                newAspNetUser.FirstName));
    }

    private async Task HandleReleaseInvites(Guid userId, string email)
    {
        var releaseInvites = await userReleaseInviteRepository.GetInvitesByEmail(email);

        await releaseInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => userReleaseRoleRepository.Create(
                userId: userId,
                releaseVersionId: invite.ReleaseVersionId,
                role: invite.Role,
                createdById: invite.CreatedById));

        await userReleaseInviteRepository.RemoveByUserEmail(email);
    }

    private async Task HandlePublicationInvites(Guid userId, string email)
    {
        var publicationInvites = await userPublicationInviteRepository.GetInvitesByEmail(email);

        await publicationInvites
            .ToAsyncEnumerable()
            .ForEachAwaitAsync(invite => userPublicationRoleRepository.Create(
                userId: userId,
                publicationId: invite.PublicationId,
                role: invite.Role,
                createdById: invite.CreatedById));

        await userPublicationInviteRepository.RemoveByUserEmail(email);
    }

    private async Task HandleExpiredInvite(
        User inviteToSystem,
        string email)
    {
        await contentDbContext.RequireTransaction(async () =>
        {
            await userReleaseInviteRepository.RemoveByUserEmail(email);
            await userPublicationInviteRepository.RemoveByUserEmail(email);

            inviteToSystem.SoftDeleted = DateTime.UtcNow;

            await contentDbContext.SaveChangesAsync();
        });
    }
}
