using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
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
        
        private static readonly Dictionary<IStatus, double> ProcessingRatios = new Dictionary<IStatus, double>() {
            {IStatus.RUNNING_PHASE_1, .1},
            {IStatus.RUNNING_PHASE_2, .1},
            {IStatus.RUNNING_PHASE_3, .1},
            {IStatus.RUNNING_PHASE_5, .7},
            {IStatus.COMPLETE, 1},
        };

        private readonly CloudTable _table;
        private readonly ILogger<ImportStatusService> _logger;

        public ImportStatusService(
            ITableStorageService tblStorageService,
            ILogger<ImportStatusService> logger)
        {
            _table = tblStorageService.GetTableAsync(DatafileImportsTableName).Result;
            _logger = logger;
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

            var percentageComplete = CalculatePercentageComplete(import.PercentageComplete, import.Status);

            _logger.LogInformation($"current status: {import.Status} : {percentageComplete}% complete");
            
            return new ImportStatus
            {
                Errors = import.Errors,
                Status = import.Status,
                NumberOfRows = import.NumberOfRows,
                PercentageComplete = percentageComplete
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
            return (int) (status switch
            {
                IStatus.RUNNING_PHASE_1 => percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_1],
                IStatus.RUNNING_PHASE_2 => ProcessingRatios[IStatus.RUNNING_PHASE_1] * 100 +
                                           (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_2]),
                IStatus.RUNNING_PHASE_3 => ProcessingRatios[IStatus.RUNNING_PHASE_1] * 100 +
                                           ProcessingRatios[IStatus.RUNNING_PHASE_2] * 100 + 
                                           (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_3]),
                IStatus.RUNNING_PHASE_4 => ProcessingRatios[IStatus.RUNNING_PHASE_1] * 100 +
                                           ProcessingRatios[IStatus.RUNNING_PHASE_2] * 100 +
                                           ProcessingRatios[IStatus.RUNNING_PHASE_3] * 100,
                IStatus.RUNNING_PHASE_5 => ProcessingRatios[IStatus.RUNNING_PHASE_1] * 100 +
                                           ProcessingRatios[IStatus.RUNNING_PHASE_2] * 100 +
                                           ProcessingRatios[IStatus.RUNNING_PHASE_3] * 100 +
                                           (percentageComplete * ProcessingRatios[IStatus.RUNNING_PHASE_5]),
                IStatus.COMPLETE => ProcessingRatios[IStatus.COMPLETE] * 100,
                _ => 0
            });
        }
    }
}