using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public class UploadStatusService : IUploadStatusService
    {
        private readonly CloudTable _table;

        public UploadStatusService(
            ITableStorageService tblStorageService)
        {
            _table = tblStorageService.GetTableAsync("imports").Result;
        }
        
        public async Task<int> GetPercentageComplete(string releaseId, string dataFileName)
        {
            var import = await GetImport(releaseId, dataFileName);
            var count = (from bool b in new BitArray(import.BatchesProcessed)
                where b
                select b).Count();
            
            var pComplete = (count * 100) / import.NumBatches;
            return pComplete;
        }
        
        public async Task CreateImport(string releaseId, string dataFileName, int numBatches)
        {
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(new DatafileImport(releaseId, dataFileName, numBatches)));
        }
        
        public async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            // Need to define the extra columns to retrieve
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId, 
                dataFileName, 
                new List<string>(){ "NumBatches", "BatchesProcessed", "Status", "Errors"}));
            
            return (DatafileImport) result.Result;
        }
    }
}