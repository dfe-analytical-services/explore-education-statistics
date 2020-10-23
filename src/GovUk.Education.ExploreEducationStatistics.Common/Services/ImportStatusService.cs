using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public enum IStatus
    {
        UPLOADING,
        QUEUED,
        PROCESSING_ARCHIVE_FILE,
        RUNNING_PHASE_1, // Basic row validation
        RUNNING_PHASE_2, // Create locations and filters
        RUNNING_PHASE_3, // Split Files
        RUNNING_PHASE_4, // Split Files complete
        RUNNING_PHASE_5, // Import observations
        COMPLETE,
        FAILED,
        NOT_FOUND
    };

    public class ImportStatusService : IImportStatusService
    {
        private static readonly List<IStatus> FinishedImportStatuses = new List<IStatus> {
            IStatus.COMPLETE,
            IStatus.FAILED,
            IStatus.NOT_FOUND
        };
        
        private static readonly Dictionary<IStatus, int> ProcessingRatios = new Dictionary<IStatus, int>() {
            {IStatus.UPLOADING, 0},
            {IStatus.QUEUED, 0},
            {IStatus.PROCESSING_ARCHIVE_FILE, 0},
            {IStatus.RUNNING_PHASE_1, 10},
            {IStatus.RUNNING_PHASE_2, 10},
            {IStatus.RUNNING_PHASE_3, 10},
            {IStatus.RUNNING_PHASE_5, 70},
            {IStatus.COMPLETE, 100},
            {IStatus.FAILED, 0},
            {IStatus.NOT_FOUND, 0}
        };

        private readonly CloudTable _table;

        public ImportStatusService(
            ITableStorageService tblStorageService)
        {
            _table = tblStorageService.GetTableAsync(DatafileImportsTableName).Result;
        }

        public async Task<ImportStatus> GetImportStatus(Guid releaseId, string dataFileName)
        {
            var import = await GetImport(releaseId, dataFileName);

            if (import.Status == IStatus.NOT_FOUND)
            {
                return new ImportStatus
                {
                    Status = import.Status
                };
            }

            return new ImportStatus
            {
                Errors = import.Errors,
                Status = import.Status,
                NumberOfRows = import.NumberOfRows,
                PercentageComplete = CalculatePercentageComplete(import.PercentageComplete, import.Status)
            };
        }

        public async Task<bool> IsImportFinished(Guid releaseId, string dataFileName)
        {
            var importStatus = await GetImportStatus(releaseId, dataFileName);

            return FinishedImportStatuses.Contains(importStatus.Status);
        }

        private async Task<DatafileImport> GetImport(Guid releaseId, string dataFileName)
        {

            // Need to define the extra columns to retrieve
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId.ToString(),
                dataFileName,
                new List<string>(){ "Status", "NumberOfRows", "Errors", "PercentageComplete"}));

            return result.Result != null ? (DatafileImport) result.Result : new DatafileImport {Status = IStatus.NOT_FOUND};
        }

        private int CalculatePercentageComplete(int percentageComplete, IStatus status)
        {
            return status switch
            {
                IStatus.RUNNING_PHASE_1 => (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_1]),
                IStatus.RUNNING_PHASE_2 => (ProcessingRatios[IStatus.RUNNING_PHASE_1] +
                                            (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_2])),
                IStatus.RUNNING_PHASE_3 => (ProcessingRatios[IStatus.RUNNING_PHASE_1] +
                                            ProcessingRatios[IStatus.RUNNING_PHASE_2] + 
                                            (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_3])),
                IStatus.RUNNING_PHASE_4 => (ProcessingRatios[IStatus.RUNNING_PHASE_1] +
                                            ProcessingRatios[IStatus.RUNNING_PHASE_2] +
                                            (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_3])),
                IStatus.RUNNING_PHASE_5 => (ProcessingRatios[IStatus.RUNNING_PHASE_1] +
                                            ProcessingRatios[IStatus.RUNNING_PHASE_2] +
                                            ProcessingRatios[IStatus.RUNNING_PHASE_3] +
                                            (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_5])),
                _ => ProcessingRatios[status]
            };
        }
    }
}