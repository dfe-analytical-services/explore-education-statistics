#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataBlockService
{
    Task<Either<ActionResult, DataBlockViewModel>> Create(Guid releaseVersionId,
        DataBlockCreateViewModel dataBlockCreate);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        Guid dataBlockVersionId);

    Task<Either<ActionResult, DataBlockViewModel>> Get(Guid dataBlockVersionId);

    Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(Guid releaseVersionId);

    Task<Either<ActionResult, DataBlockViewModel>> Update(Guid dataBlockVersionId,
        DataBlockUpdateViewModel dataBlockUpdate);

    Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan);

    Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseVersionId,
        Guid dataBlockVersionId);

    Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseVersionId,
        Subject? subject);

    Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseVersionId,
        Guid fileId);

    Task InvalidateCachedDataBlocks(Guid releaseVersionId);

    Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseVersionId);

    Task<bool> IsUnattachedDataBlock(Guid releaseVersionId,
        DataBlockVersion dataBlockVersion);

    Task<List<DataBlock>> ListDataBlocks(Guid releaseVersionId);

    Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
        Guid releaseVersionId,
        Guid dataBlockParentId);
}
