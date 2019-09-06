using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public enum IStatus
    {
        RUNNING_PHASE_1 = 1,
        RUNNING_PHASE_2 = 2,
        COMPLETE = 3,
        FAILED = 4
    };
    
    public class ImportStatusService : IImportStatusService
    {
        private readonly CloudTable _table;

        public ImportStatusService(
            ITableStorageService tblStorageService)
        {
            _table = tblStorageService.GetTableAsync("imports").Result;
        }
        
        public async Task<ImportStatus> GetImportStatus(string releaseId, string dataFileName)
        {
            var import = await GetImport(releaseId, dataFileName);
            var count = (from bool b in new BitArray(import.BatchesProcessed)
                where b
                select b).Count();
            
            return new ImportStatus
            {
                Errors = import.Errors,
                PercentageComplete = (count * 100) / import.NumBatches,
                Status = import.Status
            };
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
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