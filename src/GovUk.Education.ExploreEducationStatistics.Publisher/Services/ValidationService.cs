using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ReleaseStatus = GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseStatus;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ContentDbContext _context;
        private readonly ILogger _logger;

        public ValidationService(ContentDbContext context,
            ILogger<ValidationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages)> ValidateAsync(
            ReleaseStatusMessage statusMessage)
        {
            _logger.LogTrace($"Validating release: {statusMessage.ReleaseId}");

            var release = await GetReleaseAsync(statusMessage.ReleaseId);

            var (approvalValid, approvalMessages) = ValidateApproval(release);
            var (scheduledPublishDateValid, scheduledPublishDateMessages) = ValidateScheduledPublishDate(release);

            var valid = approvalValid &&
                        scheduledPublishDateValid;

            var logMessages = approvalMessages
                .Concat(scheduledPublishDateMessages);

            return Result(valid, logMessages);
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) ValidateApproval(Release release)
        {
            return release.Status == ReleaseStatus.Approved
                ? Success()
                : Failure(ValidationStage.ReleaseMustBeApproved, $"Release status is {release.Status}");
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) ValidateScheduledPublishDate(
            Release release)
        {
            return release.PublishScheduled.HasValue
                ? Success()
                : Failure(ValidationStage.ReleaseMustHavePublishScheduledDate, $"Scheduled publish date status is not set");
        }

        private Task<Release> GetReleaseAsync(Guid releaseId)
        {
            return _context.Releases
                .AsNoTracking()
                .Include(release => release.Publication)
                .SingleAsync(release => release.Id == releaseId);
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) Success()
        {
            return (Valid: true, LogMessages: new List<ReleaseStatusLogMessage>());
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) Failure(ValidationStage stage,
            string message)
        {
            return Result(false, $"Validating {stage.ToString()}: {message}");
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) Result(bool valid,
            string message)
        {
            return Result(valid, new List<ReleaseStatusLogMessage>
            {
                new ReleaseStatusLogMessage(message)
            });
        }

        private static (bool Valid, IEnumerable<ReleaseStatusLogMessage> LogMessages) Result(bool valid,
            IEnumerable<ReleaseStatusLogMessage> logMessages)
        {
            return (Valid: valid, LogMessages: logMessages);
        }

        private enum ValidationStage
        {
            ReleaseMustBeApproved,
            ReleaseMustHavePublishScheduledDate
        }
    }
}