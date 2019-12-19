using System;
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

        public async Task<bool> ValidateAsync(ReleaseStatusMessage statusMessage)
        {
            _logger.LogTrace($"Validating release: {statusMessage.ReleaseId}");
            var release = await GetRelease(statusMessage.ReleaseId);

            // TODO EES-869 Validate the release

            return release.Status == ReleaseStatus.Approved;
        }

        private Task<Release> GetRelease(Guid releaseId)
        {
            return _context.Releases
                .AsNoTracking()
                .SingleAsync(release => release.Id == releaseId);
        }
    }
}