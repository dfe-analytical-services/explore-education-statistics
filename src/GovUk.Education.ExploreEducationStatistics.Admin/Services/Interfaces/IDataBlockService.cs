using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataBlockService
    {
        Task<DataBlockViewModel> CreateAsync(ReleaseId releaseId, CreateDataBlockViewModel createDataBlock);

        Task<DataBlockViewModel> GetAsync(DataBlockId id);

        Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId);

        Task<DataBlockViewModel> UpdateAsync(DataBlockId id, UpdateDataBlockViewModel updateDataBlock);
    }
}