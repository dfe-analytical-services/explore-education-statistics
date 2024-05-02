using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services;

public interface IDataSetService
{
    Task<Either<ActionResult, Guid>> CreateDataSetVersion(Guid releaseFileId,
        Guid? dataSetId = null,
        CancellationToken cancellationToken = default);
}
