#nullable enable
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

        Task<Either<ActionResult, DataBlockViewModel>> Get(DataBlockId dataBlockVersionId);

        Task<Either<ActionResult, List<DataBlockSummaryViewModel>>> List(ReleaseId releaseId);

        Task<Either<ActionResult, DataBlockViewModel>> Update(DataBlockId id, DataBlockUpdateViewModel dataBlockUpdate);

        Task<Either<ActionResult, Unit>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan);

        Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(ContentSectionId releaseId, ContentSectionId dataBlockId);

        Task<DeleteDataBlockPlan> GetDeletePlan(ContentSectionId releaseId, Subject? subject);

        Task<Either<ActionResult, Unit>> RemoveChartFile(ContentSectionId releaseId, ContentSectionId id);

        Task InvalidateCachedDataBlocks(ContentSectionId releaseId);

        Task<Either<ActionResult, List<DataBlockViewModel>>> GetUnattachedDataBlocks(ContentSectionId releaseId);

        Task<bool> IsUnattachedDataBlock(ContentSectionId releaseId, DataBlockVersion dataBlockVersion);

        Task<List<DataBlock>> ListDataBlocks(ContentSectionId releaseId);

        Task<Either<ActionResult, DataBlockVersion>> GetDataBlockVersionForRelease(
            ContentSectionId releaseId,
            ContentSectionId dataBlockParentId);
    }
}
