using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IPublicationCacheService
{
    Task<Either<ActionResult, PublicationCacheViewModel>> GetPublication(string publicationSlug);

    Task<Either<ActionResult, PublicationCacheViewModel>> UpdatePublication(string publicationSlug);

    Task<Either<ActionResult, Unit>> RemovePublication(string publicationSlug);
}
