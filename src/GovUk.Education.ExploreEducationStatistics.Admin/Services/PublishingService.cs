#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationErrorMessages;
using static GovUk.Education.ExploreEducationStatistics.Admin.Validators.ValidationUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.PublisherQueues;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services
{
    public class PublishingService(
        IPersistenceHelper<ContentDbContext> persistenceHelper,
        IStorageQueueService storageQueueService,
        IUserService userService,
        ILogger<PublishingService> logger)
        : IPublishingService
    {
        /// <summary>
        /// Retry the publishing of a release version.
        /// </summary>
        /// <remarks>
        /// This results in the Publisher updating the latest ReleaseStatus for this release version rather than creating a new one.
        /// </remarks>
        /// <param name="releaseVersionId"></param>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> RetryReleasePublishing(Guid releaseVersionId)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releaseVersionId)
                .OnSuccess(userService.CheckCanPublishReleaseVersion)
                .OnSuccess(async releaseVersion =>
                {
                    if (releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
                    {
                        return ValidationActionResult(ReleaseNotApproved);
                    }

                    await storageQueueService.AddMessageAsync(
                            RetryReleasePublishingQueue,
                            new RetryReleasePublishingMessage(releaseVersionId));

                    logger.LogTrace("Sent publishing retry message for ReleaseVersion: {ReleaseVersionId}",
                        releaseVersionId);
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
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> ReleaseChanged(
            ReleasePublishingKey releasePublishingKey,
            bool immediate = false)
        {
            return await persistenceHelper
                .CheckEntityExists<ReleaseVersion>(releasePublishingKey.ReleaseVersionId)
                .OnSuccessVoid(async _ =>
                {
                    await storageQueueService.AddMessageAsync(
                        NotifyChangeQueue,
                        new NotifyChangeMessage(immediate, releasePublishingKey));

                    logger.LogTrace(
                        "Sent message for ReleaseVersion: {ReleaseVersionId}, ReleaseStatusId: {ReleaseStatusId}",
                        releasePublishingKey.ReleaseVersionId,
                        releasePublishingKey.ReleaseStatusId);
                });
        }

        /// <summary>
        /// Notify the Publisher that it should publish the Methodology files.
        /// </summary>
        /// <param name="methodologyVersionId"></param>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> PublishMethodologyFiles(Guid methodologyVersionId)
        {
            return await persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
                .OnSuccessVoid(async _ =>
                {
                    await storageQueueService.AddMessageAsync(PublishMethodologyFilesQueue,
                        new PublishMethodologyFilesMessage(methodologyVersionId));

                    logger.LogTrace("Sent message for MethodologyVersion: {MethodologyVersionId}",
                        methodologyVersionId);
                });
        }

        /// <summary>
        /// Notify the Publisher that there has been a change to the
        /// taxonomy, such as themes and topics.
        /// </summary>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> TaxonomyChanged()
        {
            await storageQueueService.AddMessageAsync(PublishTaxonomyQueue, new PublishTaxonomyMessage());

            logger.LogTrace("Sent PublishTaxonomy message");

            return Unit.Instance;
        }
    }
}
