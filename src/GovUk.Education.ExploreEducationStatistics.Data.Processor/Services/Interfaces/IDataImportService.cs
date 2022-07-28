#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IDataImportService
    {
        Task FailImport(Guid id, List<DataImportError> errors);

        Task FailImport(Guid id, params string[] errors);

        Task<DataImport> GetImport(Guid id);

        Task<DataImportStatus> GetImportStatus(Guid id);

        Task UpdateStatus(Guid id, DataImportStatus newStatus, double percentageComplete);

        Task Update(Guid id,
            int rowsPerBatch,
            int importedRows,
            int totalRows,
            int numBatches,
            HashSet<GeographicLevel> geographicLevels);
    }
}
