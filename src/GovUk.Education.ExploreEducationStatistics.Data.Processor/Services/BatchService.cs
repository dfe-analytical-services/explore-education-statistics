using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;
using static GovUk.Education.ExploreEducationStatistics.Common.TableStorageTableNames;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly CloudTable _table;

        public BatchService(ITableStorageService tblStorageService)
        {
            _table = tblStorageService.GetTableAsync(DatafileImportsTableName).Result;
        }

        public async Task UpdateStoredMessage(ImportMessage message)
        {
            var import = await GetImport(message.Release.Id, message.OrigDataFileName);
            import.Message = JsonConvert.SerializeObject(message);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }

        public async Task FailImport(Guid releaseId, string origDataFileName, IEnumerable<ValidationError> errors)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.Status != IStatus.COMPLETE && import.Status != IStatus.FAILED)
            {
                import.Status = IStatus.FAILED;
                import.Errors = JsonConvert.SerializeObject(errors);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
        }

        private async Task<DatafileImport> GetImport(Guid releaseId, string dataFileName)
        {
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId.ToString(),
                dataFileName,
                new List<string> {"NumBatches", "Status", "NumberOfRows", "Errors", "Message", "PercentageComplete"}));

            return (DatafileImport) result.Result;
        }
    }
}