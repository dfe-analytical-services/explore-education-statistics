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

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

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
    /// Retry the publishing of a release version.
    /// </summary>
    /// <remarks>
    /// This results in the Publisher updating the latest ReleaseStatus for this release version rather than creating a new one.
    /// </remarks>
    /// <param name="releaseVersionId"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> RetryReleasePublishing(Guid releaseVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccess(releaseVersion => _userService.CheckCanPublishReleaseVersion(releaseVersion))
            .OnSuccess(async releaseVersion =>
            {
                if (releaseVersion.ApprovalStatus != ReleaseApprovalStatus.Approved)
                {
                    return ValidationActionResult(ReleaseNotApproved);
                }

                await _storageQueueService.AddMessageAsync(
                    RetryReleasePublishingQueue, new RetryReleasePublishingMessage(releaseVersionId));

                _logger.LogTrace("Sent publishing retry message for ReleaseVersion: {0}", releaseVersionId);
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
    /// <param name="releaseVersionId"></param>
    /// <param name="releaseStatusId"></param>
    /// <param name="immediate">If true, runs all of the stages of the publishing workflow except that they are combined to act immediately.</param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> ReleaseChanged(Guid releaseVersionId,
        Guid releaseStatusId,
        bool immediate = false)
    {
        return await _persistenceHelper
            .CheckEntityExists<ReleaseVersion>(releaseVersionId)
            .OnSuccessVoid(async releaseVersion =>
            {
                await _storageQueueService.AddMessageAsync(
                    NotifyChangeQueue,
                    new NotifyChangeMessage(immediate,
                        releaseVersionId: releaseVersion.Id,
                        releaseStatusId: releaseStatusId));

                _logger.LogTrace("Sent message for ReleaseVersion: {0}, ReleaseStatusId: {1}", releaseVersionId,
                    releaseStatusId);
            });
    }

    /// <summary>
    /// Notify the Publisher that it should publish the Methodology files.
    /// </summary>
    /// <param name="methodologyVersionId"></param>
    /// <returns></returns>
    public async Task<Either<ActionResult, Unit>> PublishMethodologyFiles(Guid methodologyVersionId)
    {
        return await _persistenceHelper
            .CheckEntityExists<MethodologyVersion>(methodologyVersionId)
            .OnSuccessVoid(async release =>
            {
                await _storageQueueService.AddMessageAsync(PublishMethodologyFilesQueue,
                    new PublishMethodologyFilesMessage(methodologyVersionId));

                _logger.LogTrace("Sent message for Methodology: {0}", methodologyVersionId);
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
