using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.IStatus;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum IStatus
    {
        UPLOADING,
        QUEUED,
        PROCESSING_ARCHIVE_FILE,
        STAGE_1, // Basic row validation
        STAGE_2, // Create locations and filters
        STAGE_3, // Split Files
        STAGE_4, // Import observations
        COMPLETE,
        FAILED,
        NOT_FOUND,
        CANCELLING,
        CANCELLED
    }

    public class ImportStatusService : IImportStatusService
    {
        public const int STAGE_1_ROW_CHECK = 1000;
        public const int STAGE_2_ROW_CHECK = 1000;

        private static readonly List<IStatus> StatusUpdateIgnoreStates = new List<IStatus>
        {
            FAILED,
            CANCELLING,
            CANCELLED
        };
        
        private static readonly List<IStatus> FinishedImportStatuses = new List<IStatus>
        {
            COMPLETE,
            FAILED,
            NOT_FOUND,
            CANCELLED
        };

        private static readonly Dictionary<IStatus, double> ProcessingRatios = new Dictionary<IStatus, double>()
        {
            {STAGE_1, .1},
            {STAGE_2, .1},
            {STAGE_3, .1},
            {STAGE_4, .7},
            {COMPLETE, 1},
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

            if (import.Status == NOT_FOUND)
            {
                return new ImportStatus
                {
                    Status = import.Status
                };
            }

            var percentageComplete = CalculatePercentageComplete(import.PercentageComplete, import.Status);

            return new ImportStatus
            {
                Errors = import.Errors,
                Status = import.Status,
                NumberOfRows = import.NumberOfRows,
                PercentageComplete = percentageComplete,
                PhasePercentageComplete = import.PercentageComplete
            };
        }

        public async Task<bool> IsImportFinished(Guid releaseId, string dataFileName)
        {
            var importStatus = await GetImportStatus(releaseId, dataFileName);

            return FinishedImportStatuses.Contains(importStatus.Status);
        }

        public async Task UpdateStatus(Guid releaseId,
            string dataFileName,
            IStatus status,
            double percentageComplete = 0)
        {
            var import = await GetImport(releaseId, dataFileName);

            var percentageCompleteBefore = import.PercentageComplete;
            var percentageCompleteAfter = (int) Math.Clamp(percentageComplete, 0, 100);
            var statusBefore = import.Status;

            // Ignore updating when already failed, cancelled or in the process of being cancelled
            // Ignore updating to a lower status
            // Ignore updating to a lower percentage complete at the same status
            if (StatusUpdateIgnoreStates.Contains(statusBefore) 
                || statusBefore.CompareTo(status) > 0
                || statusBefore == status && percentageCompleteBefore > percentageCompleteAfter)
            {
                _logger.LogWarning(
                    $"Update: {dataFileName} {statusBefore} ({percentageCompleteBefore}%) -> {status} ({percentageCompleteAfter}%) ignored");
                return;
            }

            // Ignore updating to an equal percentage complete (after rounding) at the same status without logging it
            if (statusBefore == status && percentageCompleteBefore == percentageCompleteAfter)
            {
                return;
            }

            _logger.LogInformation(
                $"Update: {dataFileName} {statusBefore} ({percentageCompleteBefore}%) -> {status} ({percentageCompleteAfter}%)");

            import.PercentageComplete = percentageCompleteAfter;
            import.Status = status;

            await _table.ExecuteAsync(TableOperation.Merge(import));
        }

        private async Task<DatafileImport> GetImport(Guid releaseId, string dataFileName)
        {
            // Need to define the extra columns to retrieve
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId.ToString(),
                dataFileName,
                new List<string> {"NumBatches", "Status", "NumberOfRows", "Errors", "Message", "PercentageComplete"}));

            return result.Result != null
                ? (DatafileImport) result.Result
                : new DatafileImport {Status = NOT_FOUND};
        }

        private static int CalculatePercentageComplete(int percentageComplete, IStatus status)
        {
            return (int) (status switch
            {
                STAGE_1 => percentageComplete * ProcessingRatios[STAGE_1],
                STAGE_2 => ProcessingRatios[STAGE_1] * 100 +
                                   percentageComplete * ProcessingRatios[STAGE_2],
                STAGE_3 => ProcessingRatios[STAGE_1] * 100 +
                                   ProcessingRatios[STAGE_2] * 100 +
                                   percentageComplete * ProcessingRatios[STAGE_3],
                STAGE_4 => ProcessingRatios[STAGE_1] * 100 +
                                   ProcessingRatios[STAGE_2] * 100 +
                                   ProcessingRatios[STAGE_3] * 100 +
                                   percentageComplete * ProcessingRatios[STAGE_4],
                COMPLETE => ProcessingRatios[COMPLETE] * 100,
                _ => 0
            });
        }
    }
}