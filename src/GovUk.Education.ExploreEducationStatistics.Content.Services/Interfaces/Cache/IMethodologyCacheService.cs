using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IMethodologyCacheService
{
    Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree();

    Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateSummariesTree();

    Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetSummariesByPublication(
        Guid publicationId
    );
}
