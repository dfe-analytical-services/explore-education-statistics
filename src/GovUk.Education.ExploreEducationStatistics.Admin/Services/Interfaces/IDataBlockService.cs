using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
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
        Task<Either<ActionResult, DataBlockViewModel>> CreateAsync(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock);

        Task<Either<ActionResult, bool>> DeleteAsync(ReleaseId releaseId, DataBlockId id);

        Task<DataBlockViewModel> GetAsync(DataBlockId id);

        Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId);

        Task<Either<ActionResult, DataBlockViewModel>> UpdateAsync(DataBlockId id, UpdateDataBlockViewModel updateDataBlock);

        Task<Either<ActionResult, bool>> DeleteDataBlocks(DeleteDataBlockPlan deletePlan);

        Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeleteDataBlockPlan(Guid releaseId, Guid id);

        Task<DeleteDataBlockPlan> GetDeleteDataBlockPlan(Guid releaseId, Subject subject);

        Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseId, Guid id);
    }
}