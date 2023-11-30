#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;

public interface IDataGuidanceDataSetService
{
    Task<Either<ActionResult, List<DataGuidanceDataSetViewModel>>> ListDataSets(Guid releaseId,
        IList<Guid>? dataFileIds = null,
        CancellationToken cancellationToken = default);

    Task<List<string>> ListGeographicLevels(Guid subjectId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> Validate(Guid releaseId,
        CancellationToken cancellationToken = default);
}
