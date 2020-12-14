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
        Task<Either<ActionResult, DataBlockViewModel>> Create(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock);

        Task<Either<ActionResult, Unit>> Delete(ReleaseId releaseId, DataBlockId id);

        Task<Either<ActionResult, DataBlockViewModel>> Get(DataBlockId id);

        Task<Either<ActionResult, List<DataBlockViewModel>>> List(ReleaseId releaseId);

        Task<Either<ActionResult, DataBlockViewModel>> Update(DataBlockId id, UpdateDataBlockViewModel updateDataBlock);

        Task DeleteDataBlocks(DeleteDataBlockPlan deletePlan);

        Task<Either<ActionResult, DeleteDataBlockPlan>> GetDeletePlan(Guid releaseId, Guid id);

        Task<DeleteDataBlockPlan> GetDeletePlan(Guid releaseId, Subject subject);

        Task<Either<ActionResult, Unit>> RemoveChartFile(Guid releaseId, Guid id);
    }
}