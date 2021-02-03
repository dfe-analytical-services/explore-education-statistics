using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IImportService
    {
        Task FailImport(Guid id, List<ImportError> errors);

        Task FailImport(Guid id, params string[] errors);

        Task<Import> GetImport(Guid id);

        Task<ImportStatus> GetImportStatus(Guid id);

        Task UpdateStatus(Guid id, ImportStatus newStatus, double percentageComplete);

        Task Update(Guid id, int rowsPerBatch, int totalRows, int numBatches);
    }
}