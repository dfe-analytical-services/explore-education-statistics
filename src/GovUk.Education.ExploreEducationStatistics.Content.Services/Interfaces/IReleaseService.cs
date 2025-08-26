#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IReleaseService
{
    Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(string publicationSlug,
        string? releaseSlug);

    Task<Either<ActionResult, ReleaseCacheViewModel>> GetRelease(Guid releaseVersionId,
        DateTime? expectedPublishDate = null);

    Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug);
}
