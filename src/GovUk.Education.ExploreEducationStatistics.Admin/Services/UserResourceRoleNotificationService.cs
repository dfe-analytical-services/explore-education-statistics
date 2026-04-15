#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Enums;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Queries;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserResourceRoleNotificationService(
    ContentDbContext contentDbContext,
    IPreReleaseService preReleaseService,
    IUserRepository userRepository,
    IEmailTemplateService emailTemplateService,
    IUserPrereleaseRoleRepository userPrereleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository,
    TimeProvider timeProvider
) : IUserResourceRoleNotificationService
{
    public async Task NotifyUserOfInvite(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUser(userId, cancellationToken);

        if (user.Active)
        {
            throw new ArgumentException($"User with ID {userId} is already active and does not need notifying.");
        }

        var pendingUserPreReleaseRoles = await userPrereleaseRoleRepository
            .Query(ResourceRoleFilter.PendingOnly)
            .AsNoTracking()
            .WhereForUser(user.Id)
            .Select(urr => new
            {
                urr.Id,
                PublicationTitle = urr.ReleaseVersion.Release.Publication.Title,
                ReleaseTitle = urr.ReleaseVersion.Release.Title,
                urr.Role,
            })
            .ToListAsync(cancellationToken);

        var pendingUserPublicationRoles = await userPublicationRoleRepository
            .Query(ResourceRoleFilter.PendingOnly)
            .AsNoTracking()
            .WhereForUser(user.Id)
            .Select(upr => new
            {
                upr.Id,
                PublicationTitle = upr.Publication.Title,
                upr.Role,
            })
            .ToListAsync(cancellationToken);

        var preReleaseRolesInfo = pendingUserPreReleaseRoles
            .Select(urr => (urr.PublicationTitle, urr.ReleaseTitle))
            .ToHashSet();

        var publicationRolesInfo = pendingUserPublicationRoles
            .Select(upr => (upr.PublicationTitle, upr.Role))
            .ToHashSet();

        var utcNow = timeProvider.GetUtcNow();

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for multiple roles which have an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.

            await pendingUserPreReleaseRoles
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async userPrereleaseRole =>
                        await userPrereleaseRoleRepository.MarkEmailAsSent(
                            userPrereleaseRoleId: userPrereleaseRole.Id,
                            dateSent: utcNow,
                            cancellationToken: cancellationToken
                        ),
                    cancellationToken
                );

            await pendingUserPublicationRoles
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async userPublicationRole =>
                        await userPublicationRoleRepository.MarkEmailAsSent(
                            userPublicationRoleId: userPublicationRole.Id,
                            dateSent: utcNow,
                            cancellationToken: cancellationToken
                        ),
                    cancellationToken
                );

            emailTemplateService
                .SendInviteEmail(
                    email: user.Email,
                    preReleaseRolesInfo: preReleaseRolesInfo,
                    publicationRolesInfo: publicationRolesInfo
                )
                .OrThrow(_ => throw new EmailSendFailedException($"Failed to send user invite email to {user.Email}."));
        });
    }

    public async Task NotifyUserOfNewPublicationRole(
        Guid userPublicationRoleId,
        CancellationToken cancellationToken = default
    )
    {
        var userPublicationRole = await CheckUserPublicationRoleExists(userPublicationRoleId, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userPublicationRoleRepository.MarkEmailAsSent(
                userPublicationRoleId: userPublicationRoleId,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendPublicationRoleEmail(
                    email: userPublicationRole.User.Email,
                    publicationTitle: userPublicationRole.Publication.Title,
                    role: userPublicationRole.Role
                )
                .OrThrow(_ =>
                    throw new EmailSendFailedException(
                        $"Failed to send publication role email for role with ID {userPublicationRoleId}."
                    )
                );
        });
    }

    public async Task NotifyUserOfNewPreReleaseRole(
        Guid userPrereleaseRoleId,
        CancellationToken cancellationToken = default
    )
    {
        var userPrereleaseRole = await CheckUserPrereleaseRoleExists(userPrereleaseRoleId, cancellationToken);

        var isNewUser = !userPrereleaseRole.User.Active;

        var preReleaseWindow = preReleaseService.GetPreReleaseWindow(userPrereleaseRole.ReleaseVersion);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userPrereleaseRoleRepository.MarkEmailAsSent(
                userPrereleaseRoleId: userPrereleaseRoleId,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendPreReleaseInviteEmail(
                    email: userPrereleaseRole.User.Email,
                    publicationTitle: userPrereleaseRole.ReleaseVersion.Release.Publication.Title,
                    releaseTitle: userPrereleaseRole.ReleaseVersion.Release.Title,
                    isNewUser: isNewUser,
                    publicationId: userPrereleaseRole.ReleaseVersion.Release.PublicationId,
                    releaseVersionId: userPrereleaseRole.ReleaseVersionId,
                    preReleaseWindowStart: preReleaseWindow.Start,
                    publishScheduled: userPrereleaseRole.ReleaseVersion.PublishScheduled!.Value
                )
                .OrThrow(_ => new EmailSendFailedException(
                    $"Failed to send pre-release role email for role with ID {userPrereleaseRoleId}."
                ));
        });
    }

    private async Task<UserPublicationRole> CheckUserPublicationRoleExists(
        Guid userPublicationRoleId,
        CancellationToken cancellationToken
    )
    {
        return await userPublicationRoleRepository
                .Query(ResourceRoleFilter.AllButExpired)
                .AsNoTracking()
                .Include(upr => upr.User)
                .Include(upr => upr.Publication)
                .SingleOrDefaultAsync(upr => upr.Id == userPublicationRoleId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"A non-expired publication role with ID {userPublicationRoleId} does not exist."
            );
        ;
    }

    private async Task<UserReleaseRole> CheckUserPrereleaseRoleExists(
        Guid userPrereleaseRoleId,
        CancellationToken cancellationToken
    )
    {
        // Intentionally not using AsNoTracking.
        // This entity is updated in ReleaseApprovalService.CreateReleaseStatus, but SaveChanges occurs
        // after NotifyUserOfNewPreReleaseRole (in this class), which requires ReleaseVersion.PublishScheduled.
        // Using AsNoTracking causes PublishScheduled to be null.
        // Reordering to notify after SaveChanges would avoid this, but breaks
        // unit tests because the in-memory provider does not support transactions.
        return await userPrereleaseRoleRepository
                .Query(ResourceRoleFilter.AllButExpired)
                .Include(urr => urr.User)
                .Include(urr => urr.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                        .ThenInclude(r => r.Publication)
                .SingleOrDefaultAsync(urr => urr.Id == userPrereleaseRoleId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"A non-expired pre-release role with ID {userPrereleaseRoleId} does not exist."
            );
        ;
    }

    private async Task<User> FindUser(Guid userId, CancellationToken cancellationToken)
    {
        return await userRepository.FindUserById(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {userId} does not exist.");
    }
}
