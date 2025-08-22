#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IFrontendService
{
    Task<Either<ActionResult, PermalinkTableViewModel>> CreateTable(
        TableBuilderResultViewModel tableResult,
        TableBuilderConfiguration tableConfiguration,
        CancellationToken cancellationToken = default);
}
