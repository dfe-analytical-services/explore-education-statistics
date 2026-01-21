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
    IUserReleaseRoleRepository userReleaseRoleRepository,
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

        var userReleaseRoles = await userReleaseRoleRepository
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

        var userPublicationRoles = await userPublicationRoleRepository
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

        var releaseRolesInfo = userReleaseRoles
            .Select(urr => (urr.PublicationTitle, urr.ReleaseTitle, urr.Role))
            .ToHashSet();

        var publicationRolesInfo = userPublicationRoles.Select(upr => (upr.PublicationTitle, upr.Role)).ToHashSet();

        var utcNow = timeProvider.GetUtcNow();

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for multiple roles which have an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.

            await userReleaseRoles
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async userReleaseRole =>
                        await userReleaseRoleRepository.MarkEmailAsSent(
                            userReleaseRoleId: userReleaseRole.Id,
                            dateSent: utcNow,
                            cancellationToken: cancellationToken
                        ),
                    cancellationToken
                );

            await userPublicationRoles
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
                    releaseRolesInfo: releaseRolesInfo,
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

    public async Task NotifyUserOfNewReleaseRole(Guid userReleaseRoleId, CancellationToken cancellationToken = default)
    {
        var userReleaseRole = await CheckUserReleaseRoleExists(userReleaseRoleId, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userReleaseRoleRepository.MarkEmailAsSent(
                userReleaseRoleId: userReleaseRoleId,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendReleaseRoleEmail(
                    email: userReleaseRole.User.Email,
                    publicationTitle: userReleaseRole.ReleaseVersion.Release.Publication.Title,
                    releaseTitle: userReleaseRole.ReleaseVersion.Release.Title,
                    publicationId: userReleaseRole.ReleaseVersion.Release.PublicationId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    role: userReleaseRole.Role
                )
                .OrThrow(_ =>
                    throw new EmailSendFailedException(
                        $"Failed to send release role email for role with ID {userReleaseRoleId}."
                    )
                );
        });
    }

    public async Task NotifyUserOfNewContributorRoles(
        HashSet<Guid> userReleaseRoleIds,
        CancellationToken cancellationToken = default
    )
    {
        if (userReleaseRoleIds.IsNullOrEmpty())
        {
            throw new ArgumentException(
                $"User release role IDs list cannot be null or empty",
                nameof(userReleaseRoleIds)
            );
        }

        var userReleaseRoles = await CheckAllUserReleaseRolesExist(userReleaseRoleIds, cancellationToken);

        var allRolesAreForTheSameUser = userReleaseRoles.Select(urr => urr.UserId).Distinct().Count() == 1;

        if (!allRolesAreForTheSameUser)
        {
            throw new ArgumentException("Expected all provided release role IDs to correspond to the same user.");
        }

        var allRolesAreForTheSamePublication =
            userReleaseRoles.Select(urr => urr.ReleaseVersion.Release.PublicationId).Distinct().Count() == 1;

        if (!allRolesAreForTheSamePublication)
        {
            throw new ArgumentException(
                "Expected all provided release role IDs to correspond to the same publication."
            );
        }

        var allRolesAreContributorRoles = userReleaseRoles.All(urr => urr.Role == ReleaseRole.Contributor);

        if (!allRolesAreContributorRoles)
        {
            throw new ArgumentException("Expected all provided release role IDs to correspond to contributor roles.");
        }

        string email = userReleaseRoles.First().User.Email;
        string publicationTitle = userReleaseRoles.First().ReleaseVersion.Release.Publication.Title;
        var releasesInfo = userReleaseRoles
            .Select(urr => urr.ReleaseVersion.Release)
            .Distinct()
            .Select(r => (r.Year, r.TimePeriodCoverage, r.Title))
            .ToHashSet();

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userReleaseRoleIds
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(
                    async userReleaseRoleId =>
                        await userReleaseRoleRepository.MarkEmailAsSent(
                            userReleaseRoleId: userReleaseRoleId,
                            cancellationToken: cancellationToken
                        ),
                    cancellationToken
                );

            emailTemplateService
                .SendContributorInviteEmail(
                    email: email,
                    publicationTitle: publicationTitle,
                    releasesInfo: releasesInfo
                )
                .OrThrow(_ => throw new EmailSendFailedException($"Failed to send contributor invite email."));
        });
    }

    public async Task NotifyUserOfNewPreReleaseRole(
        Guid userReleaseRoleId,
        CancellationToken cancellationToken = default
    )
    {
        var userReleaseRole = await CheckUserReleaseRoleExists(userReleaseRoleId, cancellationToken);

        if (userReleaseRole.Role != ReleaseRole.PrereleaseViewer)
        {
            throw new ArgumentException(
                $"Expected role corresponding to ID {userReleaseRoleId} to be a pre-release role."
            );
        }

        var isNewUser = !userReleaseRole.User.Active;

        var preReleaseWindow = preReleaseService.GetPreReleaseWindow(userReleaseRole.ReleaseVersion);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userReleaseRoleRepository.MarkEmailAsSent(
                userReleaseRoleId: userReleaseRoleId,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendPreReleaseInviteEmail(
                    email: userReleaseRole.User.Email,
                    publicationTitle: userReleaseRole.ReleaseVersion.Release.Publication.Title,
                    releaseTitle: userReleaseRole.ReleaseVersion.Release.Title,
                    isNewUser: isNewUser,
                    publicationId: userReleaseRole.ReleaseVersion.Release.PublicationId,
                    releaseVersionId: userReleaseRole.ReleaseVersionId,
                    preReleaseWindowStart: preReleaseWindow.Start,
                    publishScheduled: userReleaseRole.ReleaseVersion.PublishScheduled!.Value
                )
                .OrThrow(_ => new EmailSendFailedException(
                    $"Failed to send pre-release role email for role with ID {userReleaseRoleId}."
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

    private async Task<UserReleaseRole> CheckUserReleaseRoleExists(
        Guid userReleaseRoleId,
        CancellationToken cancellationToken
    )
    {
        return await userReleaseRoleRepository
                .Query(ResourceRoleFilter.AllButExpired)
                .AsNoTracking()
                .Include(urr => urr.User)
                .Include(urr => urr.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
                        .ThenInclude(r => r.Publication)
                .SingleOrDefaultAsync(urr => urr.Id == userReleaseRoleId, cancellationToken)
            ?? throw new KeyNotFoundException(
                $"A non-expired release role with ID {userReleaseRoleId} does not exist."
            );
        ;
    }

    private async Task<IReadOnlyList<UserReleaseRole>> CheckAllUserReleaseRolesExist(
        HashSet<Guid> userReleaseRoleIds,
        CancellationToken cancellationToken
    )
    {
        var userReleaseRoles = await userReleaseRoleRepository
            .Query(ResourceRoleFilter.AllButExpired)
            .AsNoTracking()
            .Where(urr => userReleaseRoleIds.Contains(urr.Id))
            .Include(urr => urr.User)
            .Include(urr => urr.ReleaseVersion)
                .ThenInclude(rv => rv.Release)
                    .ThenInclude(r => r.Publication)
            .ToListAsync(cancellationToken);

        var userReleaseRoleIdsFound = userReleaseRoles.Select(urr => urr.Id).ToHashSet();

        if (userReleaseRoleIds.Any(id => !userReleaseRoleIdsFound.Contains(id)))
        {
            throw new KeyNotFoundException("One or more non-expired release roles do not exist for the provided IDs.");
        }

        return userReleaseRoles;
    }

    private async Task<User> FindUser(Guid userId, CancellationToken cancellationToken)
    {
        return await userRepository.FindUserById(userId, cancellationToken)
            ?? throw new KeyNotFoundException($"User with ID {userId} does not exist.");
    }
}
