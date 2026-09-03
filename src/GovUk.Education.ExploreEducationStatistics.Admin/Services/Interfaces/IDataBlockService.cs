#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataBlockService
{
    Task<Either<ActionResult, DataBlockViewModel>> Create(Guid releaseVersionId, DataBlockCreateRequest createRequest);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId, Guid dataBlockVersionId);

    Task<Either<ActionResult, DataBlockViewModel>> Get(Guid dataBlockVersionId);

    Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseVersionId);

    Task<Either<ActionResult, DataBlockViewModel>> Update(
        Guid dataBlockVersionId,
        DataBlockUpdateRequest updateRequest
    );

    Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlanViewModel deletePlan);

    Task<Either<ActionResult, DeleteDataBlockPlanViewModel>> GetDeletePlan(
        Guid releaseVersionId,
        Guid dataBlockVersionId
    );

    Task<DeleteDataBlockPlanViewModel> GetDeletePlan(Guid releaseVersionId, Subject? subject);

    Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseVersionId, Guid fileId);

    Task InvalidateCachedDataBlocks(Guid releaseVersionId);

    Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseVersionId);

    Task<bool> IsUnattachedDataBlock(Guid releaseVersionId, DataBlockVersion dataBlockVersion);

    Task<List<DataBlock>> ListDataBlocks(Guid releaseVersionId);

    Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
        Guid releaseVersionId,
        Guid dataBlockParentId
    );
}
