using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
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
        NOT_FOUND
    };

    public class ImportStatusService : IImportStatusService
    {
        public const int STAGE_1_ROW_CHECK = 1000;
        public const int STAGE_2_ROW_CHECK = 1000;

        private static readonly List<IStatus> FinishedImportStatuses = new List<IStatus>
        {
            IStatus.COMPLETE,
            IStatus.FAILED,
            IStatus.NOT_FOUND
        };

        private static readonly Dictionary<IStatus, double> ProcessingRatios = new Dictionary<IStatus, double>()
        {
            {IStatus.STAGE_1, .1},
            {IStatus.STAGE_2, .1},
            {IStatus.STAGE_3, .1},
            {IStatus.STAGE_4, .7},
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

        public async Task<bool> UpdateStatus(Guid releaseId, string origDataFileName, IStatus status)
        {
            var import = await GetImport(releaseId, origDataFileName);

            if (import.Status == IStatus.FAILED)
            {
                _logger.LogWarning($"Update: {origDataFileName} {import.Status} -> {status} ignored");
                return false;
            }

            if (import.Status == IStatus.COMPLETE || import.Status == status)
            {
                _logger.LogWarning($"Update: {origDataFileName} {import.Status} -> {status} ignored");
                return true;
            }

            var percentageCompleteBefore = import.PercentageComplete;
            var percentageCompleteAfter = status == IStatus.COMPLETE ? 100 : 0;
            var statusBefore = import.Status;

            _logger.LogInformation(
                $"Update: {origDataFileName} {statusBefore} ({percentageCompleteBefore}%) -> {status} ({percentageCompleteAfter}%)");

            import.PercentageComplete = percentageCompleteAfter;
            import.Status = status;

            try
            {
                await _table.ExecuteAsync(TableOperation.Replace(import));
            }
            catch (StorageException e)
            {
                if (e.RequestInformation.HttpStatusCode == (int) HttpStatusCode.PreconditionFailed)
                {
                    // If the table row has been updated in another thread subsequent
                    // to being read above then an exception will be thrown - ignore and continue.
                    // A similar approach will be required if optimistic locking is employed when & if
                    // we switch from table storage to using db tables.

                    _logger.LogWarning(e,
                        $"Precondition failure as expected while updating progress. ETag does not match for update: {origDataFileName} {statusBefore} ({percentageCompleteBefore}%) -> {status} ({percentageCompleteAfter}%)");
                }
                else
                {
                    throw;
                }
            }

            return true;
        }

        public async Task UpdateProgress(Guid releaseId, string origDataFileName, double percentageComplete)
        {
            var import = await GetImport(releaseId, origDataFileName);

            var before = import.PercentageComplete;
            var after = (int) Math.Clamp(percentageComplete, 0, 100);
            
            if (before < after)
            {
                _logger.LogInformation($"Update: {origDataFileName} {import.Status} ({before}%) -> {import.Status} ({after}%)");
                import.PercentageComplete = after;
                try
                {
                    await _table.ExecuteAsync(TableOperation.Replace(import));
                }
                catch (StorageException e)
                {
                    if (e.RequestInformation.HttpStatusCode == (int) HttpStatusCode.PreconditionFailed)
                    {
                        // Ignore - as above
                        _logger.LogWarning(e,
                            $"Precondition failure as expected while updating progress. ETag does not match for update: {origDataFileName} {import.Status} ({before}%) -> {import.Status} ({after}%)");
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else if (before != after)
            {
                _logger.LogWarning(
                    $"Ignoring attempt for {origDataFileName} {import.Status} to replace {before}% with lower value {after}%");
            }
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
                : new DatafileImport {Status = IStatus.NOT_FOUND};
        }

        private static int CalculatePercentageComplete(int percentageComplete, IStatus status)
        {
            return (int) (status switch
            {
                IStatus.STAGE_1 => percentageComplete * ProcessingRatios[IStatus.STAGE_1],
                IStatus.STAGE_2 => ProcessingRatios[IStatus.STAGE_1] * 100 +
                                   percentageComplete * ProcessingRatios[IStatus.STAGE_2],
                IStatus.STAGE_3 => ProcessingRatios[IStatus.STAGE_1] * 100 +
                                   ProcessingRatios[IStatus.STAGE_2] * 100 +
                                   percentageComplete * ProcessingRatios[IStatus.STAGE_3],
                IStatus.STAGE_4 => ProcessingRatios[IStatus.STAGE_1] * 100 +
                                   ProcessingRatios[IStatus.STAGE_2] * 100 +
                                   ProcessingRatios[IStatus.STAGE_3] * 100 +
                                   percentageComplete * ProcessingRatios[IStatus.STAGE_4],
                IStatus.COMPLETE => ProcessingRatios[IStatus.COMPLETE] * 100,
                _ => 0
            });
        }
    }
}