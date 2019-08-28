using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ImportStatus
    {
        RUNNING_PHASE_1 = 1,
        RUNNING_PHASE_2 = 2,
        COMPLETE = 3,
        FAILED = 4
    };
    
    public class BatchService : IBatchService
    {
        private readonly ILogger<IBatchService> _logger;
        private readonly CloudTable _table;

        public BatchService(
            ITableStorageService tblStorageService,
            ILogger<IBatchService> logger)
        {
            _table = tblStorageService.GetTableAsync("uploads").Result;
            _logger = logger;
        }

        public async Task UpdateBatchCount(string releaseId, int batchSize, int batchNo, string dataFileName)
        {
            var batch = GetOrCreateBatch(releaseId, dataFileName, batchSize).Result;
            var bitArray = new BitArray(batch.BatchesProcessed);
            bitArray.Set(batchNo - 1, true);
            bitArray.CopyTo(batch.BatchesProcessed, 0);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
            await IsBatchComplete(releaseId, batchSize, dataFileName);
        }
        
        public async Task<bool> IsBatchComplete(string releaseId, int batchSize, string dataFileName)
        {
            var batch = await GetOrCreateBatch(releaseId, dataFileName, batchSize);
            var count = (from bool b in new BitArray(batch.BatchesProcessed)
                where b
                select b).Count();
            
            _logger.LogInformation($"batchSize={batch.BatchSize} count={count}");

            var complete = count == batch.BatchSize;
            
            if (complete)
            {
                await UpdateStatus(releaseId, 
                    batch.Errors.Equals("") ? ImportStatus.COMPLETE : ImportStatus.FAILED,
                    dataFileName);

                if (batch.Errors.Equals(""))
                {
                    _logger.LogInformation($"All batches imported for {releaseId} : {dataFileName} with no error");
                }
                else
                {
                    _logger.LogInformation(
                        $"All batches imported for {releaseId} : {dataFileName} but with errors - check storage log"
                        );
                }
            }
            return complete;
        }

        public async Task UpdateStatus(string releaseId, int batchSize, ImportStatus status, string dataFileName)
        {
            var batch = await GetOrCreateBatch(releaseId, dataFileName, batchSize);
            batch.Status = (int)status;
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        public async Task UpdateStatus(string releaseId, ImportStatus status, string dataFileName)
        {
            await UpdateStatus(releaseId,-1, status, dataFileName);
        }
        
        public async Task FailBatch(string releaseId, List<string> errors, string dataFileName)
        {
            var batch = await GetOrCreateBatch(releaseId, dataFileName, -1);
            batch.Status = (int)ImportStatus.FAILED;
            batch.Errors = JsonConvert.SerializeObject(errors);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        public async Task LogErrors(string releaseId, List<string> errors, int batchNo, string dataFileName)
        {
            var batch = await GetOrCreateBatch(releaseId, dataFileName, -1);
            if (!batch.Errors.Equals(""))
            {
                var currentErrors = JsonConvert.DeserializeObject<List<string>>(batch.Errors);
                currentErrors.Concat(errors);
                batch.Errors = JsonConvert.SerializeObject(currentErrors);
            }
            else
            {
                batch.Errors = JsonConvert.SerializeObject(errors);
            }
            
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
            // Set this batch to processed
            await UpdateBatchCount(releaseId, -1, batchNo, dataFileName);
        }

        private async Task<Batch> GetOrCreateBatch(string releaseId, string dataFileName, int batchSize)
        {
            // Need to define the extra columns to retrieve
            var columns = new List<string>(){ "BatchSize", "BatchesProcessed", "Status", "Errors"};
            Batch batch;

            var result = await _table.ExecuteAsync(TableOperation.Retrieve<Batch>(releaseId, dataFileName, columns));
            if (result.Result == null)
            {
                batch = new Batch(releaseId, dataFileName, batchSize);
            }
            else
            {
                batch = (Batch) result.Result;
            }
            return batch;
        }
    }
}