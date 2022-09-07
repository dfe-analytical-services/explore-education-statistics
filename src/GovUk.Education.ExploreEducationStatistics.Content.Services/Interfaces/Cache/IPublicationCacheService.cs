#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IPublicationCacheService
{
    Task<Either<ActionResult, PublicationViewModel>> GetPublication(string publicationSlug);

    Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(string publicationSlug);
}
