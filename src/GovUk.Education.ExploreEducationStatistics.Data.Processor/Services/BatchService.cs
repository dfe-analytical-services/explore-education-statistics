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

        public async Task UpdateBatchCount(string releaseId, int numBatches, int batchNo, string dataFileName)
        {
            var batch = GetBatch(releaseId, dataFileName).Result;
            var bitArray = new BitArray(batch.BatchesProcessed);
            bitArray.Set(batchNo - 1, true);
            bitArray.CopyTo(batch.BatchesProcessed, 0);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
            await IsBatchComplete(releaseId, numBatches, dataFileName);
        }
        
        public async Task<bool> IsBatchComplete(string releaseId, int numBatches, string dataFileName)
        {
            var batch = await GetBatch(releaseId, dataFileName);
            var count = (from bool b in new BitArray(batch.BatchesProcessed)
                where b
                select b).Count();
            
            var complete = count == batch.NumBatches;

            if (!complete) return complete;
            
            await UpdateStatus(releaseId, 
                batch.Errors.Equals("") ? ImportStatus.COMPLETE : ImportStatus.FAILED,
                dataFileName);

            _logger.LogInformation(batch.Errors.Equals("")
                ? $"All batches imported for {releaseId} : {dataFileName} with no errors"
                : $"All batches imported for {releaseId} : {dataFileName} but with errors - check storage log");
            
            return complete;
        }

        public async Task UpdateStatus(string releaseId, ImportStatus status, string dataFileName)
        {
            var batch = await GetBatch(releaseId, dataFileName);
            batch.Status = (int)status;
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }

        public async Task FailBatch(string releaseId, List<string> errors, string dataFileName)
        {
            var batch = await GetBatch(releaseId, dataFileName);
            batch.Status = (int)ImportStatus.FAILED;
            batch.Errors = JsonConvert.SerializeObject(errors);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        public async Task LogErrors(string releaseId, List<string> errors, int batchNo, string dataFileName)
        {
            var batch = await GetBatch(releaseId, dataFileName);
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
        }

        public async Task CreateBatch(string releaseId, string dataFileName, int numBatches)
        {
            var batch = new Batch(releaseId, dataFileName, numBatches)
            {
                Status = (int) ImportStatus.RUNNING_PHASE_1
            };
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        private async Task<Batch> GetBatch(string releaseId, string dataFileName)
        {
            // Need to define the extra columns to retrieve
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<Batch>(
                releaseId, 
                dataFileName, 
                        new List<string>(){ "NumBatches", "BatchesProcessed", "Status", "Errors"}));
            
            return (Batch) result.Result;
        }
    }
}