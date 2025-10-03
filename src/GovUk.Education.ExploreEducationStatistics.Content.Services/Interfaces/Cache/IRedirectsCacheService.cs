using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IRedirectsCacheService
{
    Task<Either<ActionResult, RedirectsViewModel>> List();

    Task<Either<ActionResult, RedirectsViewModel>> UpdateRedirects();
}
