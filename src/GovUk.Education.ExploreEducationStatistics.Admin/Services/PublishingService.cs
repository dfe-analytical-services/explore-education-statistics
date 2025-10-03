#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class PublishingService(
    ContentDbContext context,
    IPublisherClient publisherClient,
    IUserService userService,
    ILogger<PublishingService> logger
) : IPublishingService
{
    /// <summary>
    /// Retry the publishing of a release version.
    /// </summary>
    /// <remarks>
    /// This results in the Publisher updating the latest ReleaseStatus for this release version rather than creating a new one.
    /// </remarks>
    /// <param name="releaseVersionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> RetryReleasePublishing(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .ReleaseVersions.FirstOrNotFoundAsync(rv => rv.Id == releaseVersionId, cancellationToken)
            .OnSuccess(userService.CheckCanPublishReleaseVersion)
            .OnSuccess(async releaseVersion =>
            {
                if (releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
                {
                    return ValidationActionResult(ReleaseNotApproved);
                }

                await publisherClient.RetryReleasePublishing(releaseVersionId, cancellationToken);

                logger.LogTrace(
                    "Sent publishing retry message for ReleaseVersion: {ReleaseVersionId}",
                    releaseVersionId
                );
                return new Either<ActionResult, Unit>(Unit.Instance);
            });
    }

    /// <summary>
    /// <para>Notify the Publisher that there has been a change to the release version's approval status.</para>
    /// <para>This could result in:</para>
    /// <list type="bullet">
    /// <item><term>Scheduling publication of a release version after approval</term></item>
    /// <item><term>Cancelling a schedule after un-approval</term></item>
    /// <item><term>Superseding an existing schedule with a new one</term></item>
    /// <item><term>Publishing a release version immediately</term></item>
    /// <item><term>Publishing a release version immediately, superseding a schedule</term></item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// Publishing will fail at the validation stage if the release version is already in the process of being published.
    /// Since the Data task deletes all existing statistical data before copying there will be downtime if this is called with a release version that is already published.
    /// A future schedule for publishing a release version that's not yet started will be cancelled.
    /// </remarks>
    /// <param name="releasePublishingKey"></param>
    /// <param name="immediate">If true, runs all of the stages of the publishing workflow except that they are combined to act immediately.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> ReleaseChanged(
        ReleasePublishingKey releasePublishingKey,
        bool immediate = false,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .ReleaseVersions.FirstOrNotFoundAsync(
                rv => rv.Id == releasePublishingKey.ReleaseVersionId,
                cancellationToken
            )
            .OnSuccessVoid(async _ =>
            {
                await publisherClient.HandleReleaseChanged(releasePublishingKey, immediate, cancellationToken);

                logger.LogTrace(
                    "Sent message for ReleaseVersion: {ReleaseVersionId}, ReleaseStatusId: {ReleaseStatusId}",
                    releasePublishingKey.ReleaseVersionId,
                    releasePublishingKey.ReleaseStatusId
                );
            });
    }

    /// <summary>
    /// Notify the Publisher that it should publish the Methodology files.
    /// </summary>
    /// <param name="methodologyVersionId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> PublishMethodologyFiles(
        Guid methodologyVersionId,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .MethodologyVersions.FirstOrNotFoundAsync(mv => mv.Id == methodologyVersionId, cancellationToken)
            .OnSuccessVoid(async _ =>
            {
                await publisherClient.PublishMethodologyFiles(methodologyVersionId, cancellationToken);

                logger.LogTrace("Sent message for MethodologyVersion: {MethodologyVersionId}", methodologyVersionId);
            });
    }

    /// <summary>
    /// Notify the Publisher that there has been a change to the
    /// taxonomy, such as themes.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> TaxonomyChanged(CancellationToken cancellationToken = default)
    {
        await publisherClient.PublishTaxonomy(cancellationToken);

        logger.LogTrace("Sent PublishTaxonomy message");

        return Unit.Instance;
    }
}
