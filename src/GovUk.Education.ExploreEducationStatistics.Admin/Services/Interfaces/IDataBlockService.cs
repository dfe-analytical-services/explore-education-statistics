using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataBlockService
    {
        Task<Either<ActionResult, DataBlockViewModel>> CreateAsync(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock);

        Task<Either<ActionResult, bool>> DeleteAsync(DataBlockId id);
        
        Task<DataBlockViewModel> GetAsync(DataBlockId id);

        Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId);

        Task<DataBlockViewModel> UpdateAsync(DataBlockId id, UpdateDataBlockViewModel updateDataBlock);
    }
}