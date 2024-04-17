#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IReleaseService
{
    Task<Either<ActionResult, IReadOnlyList<ApiDataSetCandidateViewModel>>> GetApiDataSetCandidates(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);
}
