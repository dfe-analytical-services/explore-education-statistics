#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Cache;

public interface IReleaseCacheService
{
    Task<Either<ActionResult, Unit>> RemoveRelease(string publicationSlug, string releaseSlug);
}
