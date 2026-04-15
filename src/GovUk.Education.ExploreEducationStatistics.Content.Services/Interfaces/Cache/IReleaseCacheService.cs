using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IReleaseCacheService
{
    Task<Either<ActionResult, Unit>> RemoveRelease(string publicationSlug, string releaseSlug);
}
