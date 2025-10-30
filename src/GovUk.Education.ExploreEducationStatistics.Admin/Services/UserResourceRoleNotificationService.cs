#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Exceptions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
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
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userPublicationRoleRepository.MarkEmailAsSent(
                userId: userId,
                publicationId: publication.Id,
                role: role,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendPublicationRoleEmail(email: user.Email, publication: publication, role: role)
                .OrElse(() =>
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
            // We could do something more 'proper' using a queueing mechanism, but this is sufficient for now.
            await userReleaseRoleRepository.MarkEmailAsSent(
                userId: userId,
                releaseVersionId: releaseVersion.Id,
                role: role,
                cancellationToken: cancellationToken
            );

            emailTemplateService
                .SendReleaseRoleEmail(email: user.Email, releaseVersion: releaseVersion, role: role)
                .OrElse(() =>
                    throw new EmailSendFailedException($"Failed to send release role email to {user.Email}.")
                );
        });
    }

    private async Task<User> FindActiveUser(Guid userId, CancellationToken cancellationToken)
    {
        return await userRepository.FindActiveUserById(userId, cancellationToken)
            ?? throw new ArgumentException($"Active user with ID {userId} does not exist.");
    }
}
