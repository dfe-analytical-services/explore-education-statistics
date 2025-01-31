#nullable enable
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces
{
    public interface IDataImportRepository
    {
        Task<DataImport> Add(DataImport dataImport);

        Task DeleteByFileId(Guid fileId);

        Task<DataImport?> GetByFileId(Guid fileId);

        Task<DataImportStatus> GetStatusByFileId(Guid fileId);
    }
}
