using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataBlockService
    {
        Task<DataBlockViewModel> Get(DataBlockId id);

        Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId);
    }
}