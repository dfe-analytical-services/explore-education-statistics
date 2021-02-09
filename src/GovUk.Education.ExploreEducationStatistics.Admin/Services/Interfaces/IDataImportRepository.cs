using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IDataImportRepository
    {
        Task<DataImport> Add(DataImport dataImport);

        Task DeleteByFileId(Guid fileId);

        Task<DataImport> GetByFileId(Guid fileId);

        Task<DataImportStatus> GetStatusByFileId(Guid fileId);
    }
}
