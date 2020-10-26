using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;
using IFileStorageService = GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces.IFileStorageService;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<IBatchService> _logger;
        private readonly CloudTable _table;

        public BatchService(
            ITableStorageService tblStorageService,
            IFileStorageService fileStorageService,
            ILogger<IBatchService> logger)
        {
            _table = tblStorageService.GetTableAsync(DatafileImportsTableName).Result;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task CheckComplete(string releaseId, ImportMessage message, StatisticsDbContext context)
        {
            if (message.NumBatches > 1)
            {
                await _fileStorageService.DeleteBatchFile(releaseId, message.DataFileName);
            }

            var numBatchesRemaining = await _fileStorageService.GetNumBatchesRemaining(releaseId, message.OrigDataFileName);
            
            var import = await GetImport(releaseId, message.OrigDataFileName);

            if (import.Status.Equals(IStatus.RUNNING_PHASE_5)
                && (message.NumBatches == 1 || numBatchesRemaining == 0))
            {
                var observationCount = context.Observation.Count(o => o.SubjectId.Equals(message.SubjectId));
                
                if (!observationCount.Equals(message.TotalRows))
                {
                    await FailImport(releaseId, message.OrigDataFileName,
                        new List<ValidationError>
                        {
                            new ValidationError(
                                $"Number of observations inserted ({observationCount}) " +
                                $"does not equal that expected ({message.TotalRows}) : Please delete & retry"
                            )
                        }.AsEnumerable());
                }
                else
                {
                    await UpdateStatus(releaseId, message.OrigDataFileName,
                        import.Errors.Equals("") ? IStatus.COMPLETE : IStatus.FAILED);
                }

                _logger.LogInformation(import.Errors.Equals("") && observationCount.Equals(message.TotalRows)
                    ? $"All batches imported for {releaseId} : {message.OrigDataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {message.OrigDataFileName} but with errors - check storage log");
            }
            else
            {
                var percentageComplete = (double)(message.NumBatches - numBatchesRemaining) / message.NumBatches * 100;
                await UpdateProgress(releaseId, message.OrigDataFileName, percentageComplete);
            }
        }

        public async Task<bool> UpdateStatus(string releaseId, string origDataFileName, IStatus status)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.Status == IStatus.FAILED)
            {
                return false;
            }

            if (import.Status != status)
            {
                import.PercentageComplete = status == IStatus.COMPLETE ? 100 : 0;
                import.Status = status;
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }

            return true;
        }

        public async Task UpdateStoredMessage(ImportMessage message)
        {
            var import = await GetImport(message.Release.Id.ToString(), message.OrigDataFileName);
            import.Message = JsonConvert.SerializeObject(message);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }

        public async Task<IStatus> GetStatus(string releaseId, string origDataFileName)
        {
            var import = await GetImport(releaseId, origDataFileName);
            return import.Status;
        }

        public async Task FailImport(string releaseId, string origDataFileName, IEnumerable<ValidationError> errors)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.Status != IStatus.COMPLETE && import.Status != IStatus.FAILED)
            {
                import.Status = IStatus.FAILED;
                import.Errors = JsonConvert.SerializeObject(errors);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
        }

        public async Task UpdateProgress(string releaseId, string origDataFileName, double percentageComplete)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.PercentageComplete < (int)percentageComplete)
            {
                import.PercentageComplete = (int)percentageComplete;
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));  
            }
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId,
                dataFileName,
                new List<string> {"NumBatches", "Status", "NumberOfRows", "Errors", "Message", "PercentageComplete"}));

            return (DatafileImport) result.Result;
        }
    }
}