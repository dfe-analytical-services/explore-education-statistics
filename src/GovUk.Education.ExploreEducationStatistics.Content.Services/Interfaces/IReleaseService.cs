#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces
{
    public interface IReleaseService
    {
        Task<Either<ActionResult, ReleaseViewModel>> Get(string publicationSlug, string? releaseSlug = null);

        Task<Either<ActionResult, ReleaseSummaryViewModel>> GetSummary(string publicationSlug, string? releaseSlug = null);

        Task<Either<ActionResult, List<ReleaseSummaryViewModel>>> List(string publicationSlug);

        Task<Either<ActionResult, CachedReleaseViewModel?>> FetchCachedRelease(
            string publicationSlug,
            string? releaseSlug = null);
    }
}
