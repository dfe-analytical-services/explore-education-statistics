#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IPublicationCacheService
{
    Task<Either<ActionResult, PublicationCacheViewModel>> GetPublication(string publicationSlug);

    Task<Either<ActionResult, IList<ThemeTree>>> GetPublicationTree(PublicationTreeFilter filter);

    Task<Either<ActionResult, PublicationCacheViewModel>> UpdatePublication(string publicationSlug);

    Task<IList<ThemeTree>> UpdatePublicationTree();
}
