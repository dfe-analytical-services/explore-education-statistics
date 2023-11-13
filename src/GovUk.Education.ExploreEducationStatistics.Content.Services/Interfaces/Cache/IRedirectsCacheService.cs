#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IRedirectsCacheService
{
    Task<Either<ActionResult,RedirectsViewModel>> List();

    Task<Either<ActionResult, RedirectsViewModel>> UpdateRedirects();
}
