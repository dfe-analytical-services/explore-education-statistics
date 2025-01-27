using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleasePublishingStatusOverallStage;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ContentDbContext _context;
        private readonly IReleasePublishingStatusService _releasePublishingStatusService;
        private readonly ILogger _logger;

        public ValidationService(
            ContentDbContext context,
            IReleasePublishingStatusService releasePublishingStatusService,
            ILogger<ValidationService> logger)
        {
            _context = context;
            _releasePublishingStatusService = releasePublishingStatusService;
            _logger = logger;
        }

        public async Task<Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit>> ValidateRelease(
            Guid releaseVersionId)
        {
            _logger.LogTrace("Validating release version: {0}", releaseVersionId);

            var releaseVersion = await _context.ReleaseVersions
                .AsNoTracking()
                .Include(releaseVersion => releaseVersion.Publication)
                .SingleAsync(releaseVersion => releaseVersion.Id == releaseVersionId);

            var approvalResult = ValidateApproval(releaseVersion);
            var scheduledPublishDateResult = ValidateScheduledPublishDate(releaseVersion);
            var valid = approvalResult.IsRight && scheduledPublishDateResult.IsRight;

            if (!valid)
            {
                return CollateMessages(approvalResult, scheduledPublishDateResult).ToList();
            }

            return Unit.Instance;
        }

        public async Task<bool> ValidatePublishingState(Guid releaseVersionId)
        {
            _logger.LogTrace("Validating publishing state for release version {ReleaseVersionId}", releaseVersionId);

            var releasePublishingStatuses =
                await _releasePublishingStatusService.GetAllByOverallStage(releaseVersionId, Scheduled, Started);

            // Should never happen as we mark scheduled releases as superseded prior to validation
            var scheduled = releasePublishingStatuses.FirstOrDefault(status => status.State.Overall == Scheduled);
            if (scheduled != null)
            {
                _logger.LogError(
                    "Validating {0} failed: " +
                    "Publishing is already scheduled. ReleaseStatus: {1}",
                    ValidationStage.ReleasePublishingStateNotScheduledOrStarted.ToString(),
                    scheduled.Id);
                return false;
            }

            var started = releasePublishingStatuses.FirstOrDefault(status => status.State.Overall == Started);
            if (started != null)
            {
                _logger.LogError(
                    "Validating {0} failed: " +
                    "Publishing has already started. ReleaseStatus: {1}",
                    ValidationStage.ReleasePublishingStateNotScheduledOrStarted.ToString(),
                    started.Id);
                return false;
            }

            return true;
        }

        private static Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit> ValidateApproval(
            ReleaseVersion releaseVersion)
        {
            return releaseVersion.ApprovalStatus == ReleaseApprovalStatus.Approved
                ? Unit.Instance
                : Failure(ValidationStage.ReleaseMustBeApproved,
                    $"Release approval status is {releaseVersion.ApprovalStatus}");
        }

        private static Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit> ValidateScheduledPublishDate(
            ReleaseVersion releaseVersion)
        {
            return releaseVersion.PublishScheduled.HasValue
                ? Unit.Instance
                : Failure(ValidationStage.ReleaseMustHavePublishScheduledDate, "Scheduled publish date is not set");
        }

        private static Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit> Failure(
            ValidationStage stage,
            string message)
        {
            return new List<ReleasePublishingStatusLogMessage>
            {
                new($"Validating {stage}: {message}")
            };
        }

        private static IEnumerable<ReleasePublishingStatusLogMessage> CollateMessages(
            params Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit>[] results)
        {
            return results.SelectMany(either =>
                either.IsLeft ? either.Left : new ReleasePublishingStatusLogMessage[] { });
        }

        private enum ValidationStage
        {
            ReleaseMustBeApproved,
            ReleaseMustHavePublishScheduledDate,
            ReleasePublishingStateNotScheduledOrStarted
        }
    }
}
