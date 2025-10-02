#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IPublishingService
{
    Task<Either<ActionResult, Unit>> RetryReleasePublishing(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> ReleaseChanged(
        ReleasePublishingKey releasePublishingKey,
        bool immediate = false,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> PublishMethodologyFiles(
        Guid methodologyVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, Unit>> TaxonomyChanged(CancellationToken cancellationToken = default);
}
