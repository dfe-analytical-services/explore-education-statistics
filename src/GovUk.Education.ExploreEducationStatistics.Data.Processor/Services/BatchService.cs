using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public enum ImportStatus
    {
        RUNNING = 1,
        FAILED = 2,
        COMPLETE = 3
    };
    
    public class BatchService : IBatchService
    {
        private readonly ITableStorageService _tblStorageService;

        public BatchService(ITableStorageService tblStorageService)
        {
            _tblStorageService = tblStorageService;
        }

        public async Task UpdateBatchCount(string releaseId, string subjectId, int batchSize, int batchNo)
        {
            var batch = GetOrCreateBatch(releaseId, subjectId, batchSize).Result;
            var bitArray = new BitArray(batch.BatchesProcessed);
            bitArray.Set(batchNo - 1, true);
            bitArray.CopyTo(batch.BatchesProcessed, 0);
            var table = await GetUploadsTable();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        public async Task<bool> IsBatchComplete(string releaseId, string subjectId, int batchSize)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, batchSize);
            var count = (from bool b in new BitArray(batch.BatchesProcessed)
                where b
                select b).Count();
            
            return count == batch.BatchSize;
        }

        public async Task UpdateStatus(string releaseId, string subjectId, int batchSize, ImportStatus status)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, batchSize);
            batch.Status = (int)status;
            var table = await GetUploadsTable();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        public async Task FailBatch(string releaseId, string subjectId, string errorMessage)
        {
            var batch = await GetOrCreateBatch(releaseId, subjectId, 0);
            batch.Status = (int)ImportStatus.FAILED;
            var table = await GetUploadsTable();
            await table.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }

        private async Task<Batch> GetOrCreateBatch(string releaseId, string subjectId, int batchSize)
        {
            var table = GetUploadsTable().Result;
            // Need to define the extra columns to retrieve
            var columns = new List<string>(){ "BatchSize", "BatchesProcessed", "CurrentBatchNo"};
            Batch batch;

            var result = await table.ExecuteAsync(TableOperation.Retrieve<Batch>(releaseId, subjectId, columns));
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

        private async Task<CloudTable> GetUploadsTable()
        {
            return await _tblStorageService.GetTableAsync("uploads");
        }
    }
}