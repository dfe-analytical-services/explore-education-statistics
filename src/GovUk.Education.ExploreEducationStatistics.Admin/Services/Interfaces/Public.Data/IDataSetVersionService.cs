#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionService
{
    Task<List<DataSetVersionStatusSummary>> GetStatusesForReleaseVersion(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DeleteVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, DataSetVersionSummaryViewModel>> CreateNextVersion(
        Guid releaseFileId,
        Guid dataSetId,
        CancellationToken cancellationToken = default);
}
