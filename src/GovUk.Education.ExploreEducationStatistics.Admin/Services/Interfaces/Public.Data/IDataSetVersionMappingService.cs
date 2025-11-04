using GovUk.Education.ExploreEducationStatistics.Admin.Requests.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Public.Data;

public interface IDataSetVersionMappingService
{
    Task<Either<ActionResult, LocationMappingPlan>> GetLocationMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, BatchLocationMappingUpdatesResponseViewModel>> ApplyBatchLocationMappingUpdates(
        Guid nextDataSetVersionId,
        BatchLocationMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, FilterMappingPlan>> GetFilterMappings(
        Guid nextDataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, BatchFilterOptionMappingUpdatesResponseViewModel>> ApplyBatchFilterOptionMappingUpdates(
        Guid nextDataSetVersionId,
        BatchFilterOptionMappingUpdatesRequest request,
        CancellationToken cancellationToken = default
    );

    Task<MappingStatusViewModel> GetMappingStatus(Guid dataSetVersionId, CancellationToken cancellationToken = default);
}
