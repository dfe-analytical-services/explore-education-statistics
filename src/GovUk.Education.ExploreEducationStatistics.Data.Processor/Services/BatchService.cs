using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly ITableStorageService _tblStorageService;

        public BatchService(ITableStorageService tblStorageService)
        {
            _tblStorageService = tblStorageService;
        }

        public void UpdateBatchCount(ImportMessage importMessage, string subjectId)
        {
            var batch = GetBatch(importMessage, subjectId).Result;
            var bitArray = new BitArray(batch.BatchesProcessed);
            bitArray.Set(importMessage.BatchNo, true);
            bitArray.CopyTo(batch.BatchesProcessed, 0);
            GetUploadsTable().Result.ExecuteAsync(TableOperation.InsertOrReplace(batch));
        }
        
        public bool IsBatchComplete(ImportMessage importMessage, string subjectId)
        {
            var batch = GetBatch(importMessage, subjectId).Result;
            var numProcessed = (from bool p in batch.BatchesProcessed
                where p
                select p).Count();
            return numProcessed == batch.BatchSize;
        }

        public void UpdateCurrentBatchNumber(ImportMessage importMessage)
        {
            throw new System.NotImplementedException();
        }

        private async Task<Batch> GetBatch(ImportMessage importMessage, string subjectId)
        {
            var table = GetUploadsTable().Result;
            // Need to define the extra columns to retrieve
            var columns = new List<string>(){ "BatchSize", "BatchesProcessed" };
            Batch batch;

            var result = await table.ExecuteAsync(TableOperation.Retrieve<Batch>(importMessage.Release.Id.ToString(), subjectId, columns));
            if (result == null)
            {
                batch = new Batch(importMessage.Release.Id.ToString(), subjectId, importMessage.BatchSize);
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