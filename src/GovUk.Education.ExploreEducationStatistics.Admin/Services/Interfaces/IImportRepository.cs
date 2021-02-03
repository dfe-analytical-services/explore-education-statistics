using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IImportRepository
    {
        Task<Import> Add(Import import);

        Task DeleteByFileId(Guid fileId);

        Task<Import> GetByFileId(Guid fileId);

        Task<ImportStatus> GetStatusByFileId(Guid fileId);
    }
}