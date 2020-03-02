using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services
{
    public enum IStatus
    {
        RUNNING_PHASE_1,
        RUNNING_PHASE_2,
        RUNNING_PHASE_3,
        COMPLETE,
        FAILED,
        NOT_FOUND
    };
    
    public class ImportStatusService : IImportStatusService
    {
        private static readonly List<IStatus> FinishedImportStatuses = new List<IStatus> {
            IStatus.COMPLETE,
            IStatus.FAILED
        };
        
        private readonly CloudTable _table;

        public ImportStatusService(
            ITableStorageService tblStorageService)
        {
            _table = tblStorageService.GetTableAsync("imports").Result;
        }
        
        public async Task<ImportStatus> GetImportStatus(string releaseId, string dataFileName)
        {
            var import = await GetImport(releaseId, dataFileName);

            if (import.Status == IStatus.NOT_FOUND)
            {
                return new ImportStatus
                {
                    Status = import.Status.GetEnumValue()
                };
            }

            return new ImportStatus
            {
                Errors = import.Errors,
                Status = import.Status.GetEnumValue(),
                NumberOfRows = import.NumberOfRows,
            };
        }

        public async Task<bool> IsImportFinished(string releaseId, string dataFileName)
        {
            var importStatus = await GetImportStatus(releaseId, dataFileName);
            
            return FinishedImportStatuses
                .Select(status => status.ToString())
                .ToList()
                .Contains(importStatus.Status);
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            // Need to define the extra columns to retrieve
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId, 
                dataFileName, 
                new List<string>(){ "NumBatches", "Status", "NumberOfRows", "Errors"}));
            
            return result.Result != null ? (DatafileImport) result.Result : new DatafileImport {Status = IStatus.NOT_FOUND};
        }
    }
}