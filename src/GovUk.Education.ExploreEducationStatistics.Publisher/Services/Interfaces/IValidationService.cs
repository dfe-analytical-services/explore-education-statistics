using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IValidationService
    {
        Task<bool> ValidatePublishingState(Guid releaseId);
        Task<Either<IEnumerable<ReleasePublishingStatusLogMessage>, Unit>> ValidateRelease(Guid releaseId);
    }
}