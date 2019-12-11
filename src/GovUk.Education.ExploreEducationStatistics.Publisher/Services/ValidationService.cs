using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class ValidationService : IValidationService
    {
        private readonly ILogger _logger;

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger;
        }

        public Task<bool> ValidateAsync(ValidateReleaseMessage message)
        {
            _logger.LogTrace($"Validating release: {message.ReleaseId}");
            // TODO EES-869 Validate the release
            return Task.FromResult(true);
        }
    }
}