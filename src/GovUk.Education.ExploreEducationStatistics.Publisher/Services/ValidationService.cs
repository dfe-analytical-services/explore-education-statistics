﻿using System;
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
using static GovUk.Education.ExploreEducationStatistics.Publisher.Model.ReleaseStatusOverallStage;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ContentDbContext _context;
        private readonly IReleaseStatusService _releaseStatusService;
        private readonly ILogger _logger;

        public ValidationService(ContentDbContext context,
            IReleaseStatusService releaseStatusService,
            ILogger<ValidationService> logger)
        {
            _context = context;
            _releaseStatusService = releaseStatusService;
            _logger = logger;
        }

        public async Task<Either<IEnumerable<ReleaseStatusLogMessage>, Unit>> ValidateRelease(Guid releaseId)
        {
            _logger.LogTrace($"Validating release: {releaseId}");

            var release = await GetReleaseAsync(releaseId);

            var approvalResult = ValidateApproval(release);
            var scheduledPublishDateResult = ValidateScheduledPublishDate(release);
            var valid = approvalResult.IsRight && scheduledPublishDateResult.IsRight;

            if (!valid)
            {
                return CollateMessages(approvalResult, scheduledPublishDateResult).ToList();
            }

            return Unit.Instance;
        }

        public async Task<bool> ValidatePublishingState(Guid releaseId)
        {
            _logger.LogTrace($"Validating publishing state: {releaseId}");

            var releaseStatuses =
                (await _releaseStatusService.GetAllByOverallStage(releaseId, Scheduled, Started)).ToList();

            // Should never happen as we mark scheduled releases as superseded prior to validation
            var scheduled = releaseStatuses.FirstOrDefault(status => status.State.Overall == Scheduled);
            if (scheduled != null)
            {
                _logger.LogError(
                    $"Validating {ValidationStage.ReleasePublishingStateNotScheduledOrStarted.ToString()} failed: " +
                    $"Publishing is already scheduled. ReleaseStatus: {scheduled.Id}");
                return false;
            }

            var started = releaseStatuses.FirstOrDefault(status => status.State.Overall == Started);
            if (started != null)
            {
                _logger.LogError(
                    $"Validating {ValidationStage.ReleasePublishingStateNotScheduledOrStarted.ToString()} failed: " +
                    $"Publishing has already started. ReleaseStatus: {started.Id}");
                return false;
            }

            return true;
        }

        private static Either<IEnumerable<ReleaseStatusLogMessage>, Unit> ValidateApproval(Release release)
        {
            return release.Status == ReleaseStatus.Approved
                ? Unit.Instance
                : Failure(ValidationStage.ReleaseMustBeApproved, $"Release status is {release.Status}");
        }

        private static Either<IEnumerable<ReleaseStatusLogMessage>, Unit> ValidateScheduledPublishDate(
            Release release)
        {
            return release.PublishScheduled.HasValue
                ? Unit.Instance
                : Failure(ValidationStage.ReleaseMustHavePublishScheduledDate, "Scheduled publish date is not set");
        }

        private Task<Release> GetReleaseAsync(Guid releaseId)
        {
            return _context.Releases
                .AsNoTracking()
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseId);
        }

        private static Either<IEnumerable<ReleaseStatusLogMessage>, Unit> Failure(ValidationStage stage, string message)
        {
            return new List<ReleaseStatusLogMessage>
            {
                new ReleaseStatusLogMessage($"Validating {stage.ToString()}: {message}")
            };
        }

        private static IEnumerable<ReleaseStatusLogMessage> CollateMessages(
            params Either<IEnumerable<ReleaseStatusLogMessage>, Unit>[] results)
        {
            return results.SelectMany(either => either.IsLeft ? either.Left : new ReleaseStatusLogMessage[] {});
        }

        private enum ValidationStage
        {
            ReleaseMustBeApproved,
            ReleaseMustHavePublishScheduledDate,
            ReleasePublishingStateNotScheduledOrStarted
        }
    }
}