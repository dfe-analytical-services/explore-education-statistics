using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IReleaseCacheService
{
    Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug, string? releaseSlug = null);

    Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateRelease(
        Guid releaseVersionId,
        string publicationSlug,
        string? releaseSlug = null
    );

    Task<Either<ActionResult, ReleaseCacheViewModel>> UpdateReleaseStaged(
        Guid releaseVersionId,
        DateTimeOffset expectedPublishDate,
        string publicationSlug,
        string? releaseSlug = null
    );

    Task<Either<ActionResult, Unit>> RemoveRelease(string publicationSlug, string releaseSlug);
}
