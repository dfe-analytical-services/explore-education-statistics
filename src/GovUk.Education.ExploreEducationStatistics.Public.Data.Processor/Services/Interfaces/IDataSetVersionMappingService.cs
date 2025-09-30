using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Requests;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetVersionMappingService
{
    Task<Either<ActionResult, Unit>> CreateMappings(
        Guid nextDataSetVersionId,
        Guid? dataSetVersionToReplace,
        CancellationToken cancellationToken = default);

    Task<Either<ActionResult, Tuple<DataSetVersion, DataSetVersionImport>>> GetManualMappingVersionAndImport(
        NextDataSetVersionCompleteImportRequest request,
        CancellationToken cancellationToken = default);

    Task ApplyAutoMappings(
        Guid nextDataSetVersionId,
        bool isReplacement = false,
        CancellationToken cancellationToken = default);
}
