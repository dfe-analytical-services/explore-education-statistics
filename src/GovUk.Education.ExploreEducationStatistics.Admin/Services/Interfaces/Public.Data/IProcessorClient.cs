#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IProcessorClient
{
    Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateDataSet(
        Guid releaseFileId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, ProcessDataSetVersionResponseViewModel>> CreateNextDataSetVersion(
        Guid dataSetId,
        Guid releaseFileId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> BulkDeleteDataSetVersions(
        Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Unit>> DeleteDataSetVersion(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
