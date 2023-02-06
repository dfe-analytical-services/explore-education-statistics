#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;
using Unit = GovUk.Education.ExploreEducationStatistics.Common.Model.Unit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataBlockService
    {
        Task<Either<ActionResult, DataBlockViewModel>> Create(ReleaseId releaseId, DataBlockCreateViewModel dataBlockCreate);

        Task<Either<ActionResult, Unit>> Delete(ReleaseId releaseId, DataBlockId id);

        Task<Either<ActionResult, DataBlockViewModel>> Get(DataBlockId id);

        Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(ReleaseId releaseId);

        Task<Either<ActionResult, DataBlockViewModel>> Update(DataBlockId id, DataBlockUpdateViewModel dataBlockUpdate);

        Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan);

        Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id);

        Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseId, Subject? subject);

        Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseId, Guid id);

        Task InvalidateCachedDataBlocks(Guid releaseId);

        Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(Guid releaseId);

        Task<bool> IsUnattachedDataBlock(Guid releaseId, DataBlock dataBlock);
    }
}
