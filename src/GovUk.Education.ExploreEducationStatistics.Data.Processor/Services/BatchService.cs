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

        public async Task UpdateBatchCount(string releaseId, string subjectId, int batchSize, int batchNo)
        {
            var batch = GetOrCreateBatch(releaseId, subjectId, batchSize).Result;
            var bitArray = new BitArray(batch.BatchesProcessed);
            bitArray.Set(batchNo - 1, true);
            bitArray.CopyTo(batch.BatchesProcessed, 0);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
            await IsBatchComplete(releaseId, subjectId, batchSize);
        }
        
        public async Task<bool> IsBatchComplete(string releaseId, string subjectId, int batchSize)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, batchSize);
            var count = (from bool b in new BitArray(batch.BatchesProcessed)
                where b
                select b).Count();
            
            _logger.LogInformation($"batchSize={batch.BatchSize} count={count}");

            var complete = count == batch.BatchSize;
            
            if (complete)
            {
                await UpdateStatus(releaseId, subjectId, 
                    batch.Errors.Equals("") ? ImportStatus.COMPLETE : ImportStatus.FAILED);

                if (batch.Errors.Equals(""))
                {
                    _logger.LogInformation($"All batches imported for {releaseId} : {subjectId} with no error");
                }
                else
                {
                    _logger.LogInformation(
                        $"All batches imported for {releaseId} : {subjectId} but with errors - check storage log"
                        );
                }
            }
            return complete;
        }

        public async Task UpdateStatus(string releaseId, string subjectId, int batchSize, ImportStatus status)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, batchSize);
            batch.Status = (int)status;
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        public async Task UpdateStatus(string releaseId, string subjectId, ImportStatus status)
        {
            await UpdateStatus(releaseId, subjectId, -1, status);
        }
        
        public async Task FailBatch(string releaseId, string subjectId, List<string> errors)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, -1);
            batch.Status = (int)ImportStatus.FAILED;
            batch.Errors = JsonConvert.SerializeObject(errors);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        public async Task LogErrors(string releaseId, string subjectId, List<string> errors, int batchNo)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, -1);
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
            await UpdateBatchCount(releaseId, subjectId, -1, batchNo);
        }

        private async Task<Batch> GetOrCreateBatch(string releaseId, string subjectId, int batchSize)
        {
            // Need to define the extra columns to retrieve
            var columns = new List<string>(){ "BatchSize", "BatchesProcessed", "Status", "Errors"};
            Batch batch;

            var result = await _table.ExecuteAsync(TableOperation.Retrieve<Batch>(releaseId, subjectId, columns));
            if (result.Result == null)
            {
                batch = new Batch(releaseId, subjectId, batchSize);
            }
            else
            {
                batch = (Batch) result.Result;
            }
            return batch;
        }
    }
}