using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IDataSetVersionChangeService
{
    Task<Either<ActionResult, DataSetVersionChangesViewModel>> GetChanges(
        Guid dataSetId,
        string dataSetVersion,
        bool includePatchHistory,
        CancellationToken cancellationToken = default
    );
}
