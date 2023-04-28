#nullable enable
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Api.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces;

public interface IFrontendService
{
    Task<Either<ActionResult, dynamic>> CreateUniversalTable(LegacyPermalink legacyPermalink,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, dynamic>> CreateUniversalTable(TableBuilderResultViewModel tableResult,
        TableBuilderConfiguration tableConfiguration,
        CancellationToken cancellationToken = default);
}
