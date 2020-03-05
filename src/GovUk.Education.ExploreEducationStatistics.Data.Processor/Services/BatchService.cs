using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportStatusService _importStatusService;
        private readonly ILogger<IBatchService> _logger;
        private readonly CloudTable _table;

        public BatchService(
            ITableStorageService tblStorageService,
            IFileStorageService fileStorageService,
            IImportStatusService importStatusService,
            ILogger<IBatchService> logger)
        {
            _table = tblStorageService.GetTableAsync("imports").Result;
            _fileStorageService = fileStorageService;
            _importStatusService = importStatusService;
            _logger = logger;
        }

        public async Task CheckComplete(string releaseId, ImportMessage message)
        {
            var import = await GetImport(releaseId, message.OrigDataFileName);
            
            if (message.NumBatches > 1)
            {
                _fileStorageService.DeleteBatchFile(releaseId, message.DataFileName);
            }
            
            if (message.NumBatches == 1 || _fileStorageService.GetNumBatchesRemaining(releaseId, message.OrigDataFileName) == 0)
            {
                await UpdateStatus(releaseId, message.OrigDataFileName,
                    import.Errors.Equals("") ? IStatus.COMPLETE : IStatus.FAILED
                );

                _logger.LogInformation(import.Errors.Equals("")
                    ? $"All batches imported for {releaseId} : {message.OrigDataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {message.OrigDataFileName} but with errors - check storage log");
            }
        }

        public async Task<bool> UpdateStatus(string releaseId, string origDataFileName, IStatus status)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.Status == IStatus.FAILED)
            {
                return false;
            }

            import.Status = status;
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));

            return true;
        }
        
        public async Task<IStatus> GetStatus(string releaseId, string origDataFileName)
        {
            var import = await GetImport(releaseId, origDataFileName);
            return import.Status;
        }

        public async Task FailImport(string releaseId, string origDataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, origDataFileName);
            if (import.Status != IStatus.COMPLETE)
            {
                import.Status = IStatus.FAILED;
                import.Errors = JsonConvert.SerializeObject(errors);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId,
                dataFileName,
                new List<string> {"NumBatches", "Status", "NumberOfRows", "Errors", "Message"}));

            return (DatafileImport) result.Result;
        }
    }
}