using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Models.Api;
using ContentSectionId = System.Guid;
using DataBlockId = System.Guid;
using ReleaseId = System.Guid;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataBlockService
    {
        Task<Either<ValidationResult, DataBlockViewModel>> CreateAsync(ReleaseId releaseId, 
            ContentSectionId contentSectionId, CreateDataBlockViewModel createDataBlock);

        Task<DataBlockViewModel> GetAsync(ReleaseId releaseId, DataBlockId id);

        Task<List<DataBlockViewModel>> ListAsync(ReleaseId releaseId);
    }
}