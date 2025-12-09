#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class UserResourceRoleNotificationService(
    ContentDbContext contentDbContext,
    IUserRepository userRepository,
    IEmailTemplateService emailTemplateService,
    IUserReleaseRoleRepository userReleaseRoleRepository,
    IUserPublicationRoleRepository userPublicationRoleRepository
) : IUserResourceRoleNotificationService
{
    public async Task NotifyUserOfNewPublicationRole(
        Guid userId,
        Publication publication,
        PublicationRole role,
        CancellationToken cancellationToken = default
    )
    {
        var user = await FindActiveUser(userId, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userPublicationRoleRepository.MarkEmailAsSent(
                userId: userId,
                publicationId: publication.Id,
                role: role,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendPublicationRoleEmail(email: user.Email, publication: publication, role: role)
                .OrThrow(_ =>
                    throw new EmailSendFailedException($"Failed to send publication role email to {user.Email}.")
                );
        });
    }

    public async Task NotifyUserOfNewReleaseRole(
        Guid userId,
        ReleaseVersion releaseVersion,
        ReleaseRole role,
        CancellationToken cancellationToken = default
    )
    {
        var user = await FindActiveUser(userId, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userReleaseRoleRepository.MarkEmailAsSent(
                userId: userId,
                releaseVersionId: releaseVersion.Id,
                role: role,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendReleaseRoleEmail(email: user.Email, releaseVersion: releaseVersion, role: role)
                .OrThrow(_ =>
                    throw new EmailSendFailedException($"Failed to send release role email to {user.Email}.")
                );
        });
    }

    public async Task NotifyUserOfNewContributorRoles(
        Guid userId,
        string publicationTitle,
        HashSet<Guid> releaseVersionIds,
        CancellationToken cancellationToken = default
    )
    {
        var user = await FindUser(userId, cancellationToken);

        await contentDbContext.RequireTransaction(async () =>
        {
            // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
            // But if we did it the other way, and the database transaction failed after sending the email, then we
            // would have sent an email for a role which has an unmarked `SentDate`.
            // At least this way, if the email fails to send, the database transaction will be rolled back.
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await releaseVersionIds
                .ToAsyncEnumerable()
                .ForEachAwaitAsync(async releaseVersionId =>
                    await userReleaseRoleRepository.MarkEmailAsSent(
                        userId: userId,
                        releaseVersionId: releaseVersionId,
                        role: ReleaseRole.Contributor,
                        cancellationToken: cancellationToken
                    )
                );

            await emailTemplateService
                .SendContributorInviteEmail(
                    email: user.Email,
                    publicationTitle: publicationTitle,
                    releaseVersionIds: releaseVersionIds
                )
                .OrThrow(_ =>
                    throw new EmailSendFailedException($"Failed to send contributor invite email to {user.Email}.")
                );
        });
    }

    public async Task NotifyUserOfNewPreReleaseRole(
        string userEmail,
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        var activeUser = await userRepository.FindActiveUserByEmail(userEmail, cancellationToken);

        var isNewUser = activeUser is null;

        await contentDbContext.RequireTransaction(async () =>
        {
            // This 'if' statement will be removed in EES-6511 when we stop using UserReleaseRoleInvites/UserPublicationRoleInvites
            // altogether. At that point, a role will always exist when we send this email.
            if (!isNewUser)
            {
                // Doing it in this order will technically set a `SentDate` slightly before the email is actually sent.
                // But if we did it the other way, and the database transaction failed after sending the email, then we
                // would have sent an email for a role which has an unmarked `SentDate`.
                // At least this way, if the email fails to send, the database transaction will be rolled back.
                // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
                await userReleaseRoleRepository.MarkEmailAsSent(
                    userId: activeUser!.Id,
                    releaseVersionId: releaseVersionId,
                    role: ReleaseRole.PrereleaseViewer,
                    cancellationToken: cancellationToken
                );
            }

            await emailTemplateService
                .SendPreReleaseInviteEmail(email: userEmail, releaseVersionId: releaseVersionId, isNewUser: isNewUser)
                .OrThrow(_ => new EmailSendFailedException($"Failed to send pre-release role email to {userEmail}."));
        });
    }

    private async Task<User> FindActiveUser(Guid userId, CancellationToken cancellationToken)
    {
        return await userRepository.FindActiveUserById(userId, cancellationToken)
            ?? throw new ArgumentException($"Active user with ID {userId} does not exist.");
    }

    private async Task<User> FindUser(Guid userId, CancellationToken cancellationToken)
    {
        return await userRepository.FindUserById(userId, cancellationToken)
            ?? throw new ArgumentException($"User with ID {userId} does not exist.");
    }
}
