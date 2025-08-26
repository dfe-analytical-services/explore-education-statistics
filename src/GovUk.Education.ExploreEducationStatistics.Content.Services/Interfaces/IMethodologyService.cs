#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IMethodologyService
{
    Task<Either<ActionResult, MethodologyVersionViewModel>> GetLatestMethodologyBySlug(string slug);

    Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree();

    Task<Either<ActionResult, List<MethodologySitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken = default);
}
