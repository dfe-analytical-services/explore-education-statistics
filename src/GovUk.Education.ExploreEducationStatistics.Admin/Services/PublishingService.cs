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
    public class PublishingService : IPublishingService
    {
        private readonly IPersistenceHelper<ContentDbContext> _persistenceHelper;
        private readonly IStorageQueueService _storageQueueService;
        private readonly IUserService _userService;
        private readonly ILogger _logger;

        public PublishingService(IPersistenceHelper<ContentDbContext> persistenceHelper,
            IStorageQueueService storageQueueService,
            IUserService userService,
            ILogger<PublishingService> logger)
        {
            _persistenceHelper = persistenceHelper;
            _storageQueueService = storageQueueService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retry the publishing of a Release.
        /// </summary>
        /// <remarks>
        /// This results in the Publisher updating the latest ReleaseStatus for this Release rather than creating a new one.
        /// </remarks>
        /// <param name="releaseId"></param>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> RetryReleasePublishing(Guid releaseId)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccess(release => _userService.CheckCanPublishRelease(release))
                .OnSuccess(async release =>
                {
                    if (release.ApprovalStatus != ReleaseApprovalStatus.Approved)
                    {
                        return ValidationActionResult(ReleaseNotApproved);
                    }

                    await _storageQueueService.AddMessageAsync(
                        RetryReleasePublishingQueue, new RetryReleasePublishingMessage(releaseId));

                    _logger.LogTrace("Sent publishing retry message for Release: {0}", releaseId);
                    return new Either<ActionResult, Unit>(Unit.Instance);
                });
        }

        /// <summary>
        /// <para>Notify the Publisher that there has been a change to the Release approval status.</para>
        /// <para>This could result in:</para>
        /// <list type="bullet">
        /// <item><term>Scheduling publication of a Release after approval</term></item>
        /// <item><term>Cancelling a schedule after un-approval</term></item>
        /// <item><term>Superseding an existing schedule with a new one</term></item>
        /// <item><term>Publishing a release immediately</term></item>
        /// <item><term>Publishing a release immediately superseding a schedule</term></item>
        /// </list>
        /// </summary>
        /// <remarks>
        /// Publishing will fail at the validation stage if the Release is already in the process of being published.
        /// Since the Data task deletes all existing Release statistics data before copying there will be downtime if this is called with a Release that is already published.
        /// A future schedule for publishing a Release that's not yet started will be cancelled.
        /// </remarks>
        /// <param name="releaseId"></param>
        /// <param name="releaseStatusId"></param>
        /// <param name="immediate">If true, runs all of the stages of the publishing workflow except that they are combined to act immediately.</param>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> ReleaseChanged(Guid releaseId, Guid releaseStatusId, bool immediate = false)
        {
            return await _persistenceHelper
                .CheckEntityExists<Release>(releaseId)
                .OnSuccessVoid(async release =>
                {
                    await _storageQueueService.AddMessageAsync(
                        NotifyChangeQueue, new NotifyChangeMessage(immediate, release.Id, releaseStatusId));

                    _logger.LogTrace("Sent message for Release: {0}, ReleaseStatusId: {1}", releaseId, releaseStatusId);
                });
        }

        /// <summary>
        /// Notify the Publisher that it should publish the Methodology files.
        /// </summary>
        /// <param name="methodologyId"></param>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> PublishMethodologyFiles(Guid methodologyId)
        {
            return await _persistenceHelper
                .CheckEntityExists<MethodologyVersion>(methodologyId)
                .OnSuccessVoid(async release =>
                {
                    await _storageQueueService.AddMessageAsync(PublishMethodologyFilesQueue,
                        new PublishMethodologyFilesMessage(methodologyId));

                    _logger.LogTrace("Sent message for Methodology: {0}", methodologyId);
                });
        }

        /// <summary>
        /// Notify the Publisher that there has been a change to the
        /// taxonomy, such as themes and topics.
        /// </summary>
        /// <returns></returns>
        public async Task<Either<ActionResult, Unit>> TaxonomyChanged()
        {
            await _storageQueueService.AddMessageAsync(PublishTaxonomyQueue, new PublishTaxonomyMessage());

            _logger.LogTrace("Sent PublishTaxonomy message");

            return Unit.Instance;
        }
    }
}
