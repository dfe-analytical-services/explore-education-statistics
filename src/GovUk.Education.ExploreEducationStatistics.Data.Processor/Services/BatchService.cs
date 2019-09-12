using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services
{
    public class BatchService : IBatchService
    {
        private readonly ILogger<IBatchService> _logger;
        private readonly CloudTable _table;
        private readonly IFileStorageService _fileStorageService;
        private readonly IImportStatusService _importStatusService;

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

        public async Task UpdateBatchCount(string releaseId, string dataFileName, int batchNo)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetCloudBlockBlob(releaseId, dataFileName);
            var leaseId = await GetLease(cloudBlockBlob);
            DatafileImport import;

            try
            {
                import = GetImport(releaseId, dataFileName).Result;
                var bitArray = new BitArray(import.BatchesProcessed);
                bitArray.Set(batchNo - 1, true);
                bitArray.CopyTo(import.BatchesProcessed, 0);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }

            if (_importStatusService.GetImportStatus(releaseId, dataFileName).Result.PercentageComplete == 100)
            {
                await UpdateStatus(releaseId, dataFileName,
                    import.Errors.Equals("") ? IStatus.COMPLETE : IStatus.FAILED
                );

                _logger.LogInformation(import.Errors.Equals("")
                    ? $"All batches imported for {releaseId} : {dataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {dataFileName} but with errors - check storage log");
            }
        }

        public async Task UpdateStatus(string releaseId, string dataFileName, IStatus status)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetCloudBlockBlob(releaseId, dataFileName);
            var leaseId = await GetLease(cloudBlockBlob);

            try
            {
                var import = await GetImport(releaseId, dataFileName);
                import.Status = status;
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }
        }

        public async Task FailImport(string releaseId, string dataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, dataFileName);
            import.Status = IStatus.FAILED;
            import.Errors = JsonConvert.SerializeObject(errors);
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }

        public async Task LogErrors(string releaseId, string dataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, dataFileName);
            if (!import.Errors.Equals(""))
            {
                var currentErrors = JsonConvert.DeserializeObject<List<string>>(import.Errors);
                currentErrors.Concat(errors);
                import.Errors = JsonConvert.SerializeObject(currentErrors);
            }
            else
            {
                import.Errors = JsonConvert.SerializeObject(errors);
            }

            await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
        }

        public async Task<bool> IsBatchProcessed(string releaseId, string dataFileName, int batchNo)
        {
            var import = await GetImport(releaseId, dataFileName);
            var bitArray = new BitArray(import.BatchesProcessed);
            return bitArray.Get(batchNo - 1);
        }

        public async Task CreateImport(string releaseId, string dataFileName, int numBatches)
        {
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DatafileImport(releaseId, dataFileName, numBatches))
            );
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId,
                dataFileName,
                new List<string>() {"NumBatches", "BatchesProcessed", "Status", "Errors"}));

            return (DatafileImport) result.Result;
        }

        private async Task<string> GetLease(CloudBlockBlob cloudBlockBlob)
        {
            string leaseId = null;

            // TODO Improve error handling & max retries 
            while (leaseId == null)
            {
                try
                {
                    leaseId = await _fileStorageService.GetLeaseId(cloudBlockBlob);
                }
                catch (Microsoft.WindowsAzure.Storage.StorageException se)
                {
                    var response = se.RequestInformation.HttpStatusCode;
                    if (response != null && (response == (int) HttpStatusCode.Conflict))
                    {
                        // A Conflict has been found, lease is being used by another process
                        // wait and try again.
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        throw se;
                    }
                }
            }

            return leaseId;
        }
    }
}