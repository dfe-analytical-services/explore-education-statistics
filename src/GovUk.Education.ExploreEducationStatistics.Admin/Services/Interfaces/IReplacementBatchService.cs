#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReplacementBatchService
{
    Task<Either<ActionResult, Unit>> Replace(Guid releaseVersionId,
        IEnumerable<Guid> originalFileIds,
        CancellationToken cancellationToken = default);
}
