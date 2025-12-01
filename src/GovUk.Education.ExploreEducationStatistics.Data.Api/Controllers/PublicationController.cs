#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Cache;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Controllers;

[Route("api")]
[ApiController]
public class PublicationController(
    IPersistenceHelper<ContentDbContext> contentPersistenceHelper,
    IReleaseService releaseService,
    ICacheKeyService cacheKeyService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<PublicationController> logger
) : ControllerBase
{
    [HttpGet("publications/{publicationId:guid}/subjects")]
    public async Task<ActionResult<List<SubjectViewModel>>> ListLatestReleaseSubjects(Guid publicationId)
    {
        return await GetLatestPublishedReleaseVersionId(publicationId)
            .OnSuccess(cacheKeyService.CreateCacheKeyForReleaseSubjects)
            .OnSuccess(ListLatestReleaseSubjects)
            .HandleFailuresOrOk();
    }

    [HttpGet("publications/{publicationId:guid}/featured-tables")]
    public async Task<ActionResult<List<FeaturedTableViewModel>>> ListLatestReleaseFeaturedTables(Guid publicationId)
    {
        return await GetLatestPublishedReleaseVersionId(publicationId)
            .OnSuccess(releaseService.ListFeaturedTables)
            .HandleFailuresOrOk();
    }

    private Task<Either<ActionResult, Guid>> GetLatestPublishedReleaseVersionId(Guid publicationId)
    {
        return contentPersistenceHelper
            .CheckEntityExists<Publication>(publicationId)
            .OnSuccess(publication =>
                publication.LatestPublishedReleaseVersionId ?? new Either<ActionResult, Guid>(new NotFoundResult())
            );
    }

    private Task<Either<ActionResult, List<SubjectViewModel>>> ListLatestReleaseSubjects(
        ReleaseSubjectsCacheKey cacheKey
    )
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: cacheKey,
            createIfNotExistsFn: () => releaseService.ListSubjects(cacheKey.ReleaseVersionId),
            logger: logger
        );
    }
}
