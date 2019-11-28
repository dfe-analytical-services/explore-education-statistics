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
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Utils;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StorageException = Microsoft.WindowsAzure.Storage.StorageException;

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

        public async Task UpdateBatchCount(string releaseId, string dataFileName, int batchNo)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetBlobReference(releaseId, dataFileName);
            var leaseId = await GetLease(cloudBlockBlob);
            DatafileImport import;

            try
            {
                import = await GetImport(releaseId, dataFileName);
                var bitArray = new BitArray(import.BatchesProcessed);
                bitArray.Set(batchNo - 1, true);
                bitArray.CopyTo(import.BatchesProcessed, 0);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }

            var result = await _importStatusService.GetImportStatus(releaseId, dataFileName);
            if (result.PercentageComplete == 100)
            {
                await UpdateStatus(releaseId, dataFileName,
                    import.Errors.Equals("") ? IStatus.COMPLETE : IStatus.FAILED
                );
                cloudBlockBlob.FetchAttributes();
                _fileStorageService.DeleteBatches(releaseId,  BlobUtils.GetMetaFileName(cloudBlockBlob));

                _logger.LogInformation(import.Errors.Equals("")
                    ? $"All batches imported for {releaseId} : {dataFileName} with no errors"
                    : $"All batches imported for {releaseId} : {dataFileName} but with errors - check storage log");
            }
        }

        public async Task<bool> UpdateStatus(string releaseId, string dataFileName, IStatus status)
        {
            // Note that optimistic locking applies to azure table so to avoid concurrency issue acquire a lease on the block blob 
            var cloudBlockBlob = _fileStorageService.GetBlobReference(releaseId, dataFileName);
            var leaseId = await GetLease(cloudBlockBlob);

            try
            {
                var import = await GetImport(releaseId, dataFileName);
                if (import.Status == IStatus.FAILED)
                {
                    return false;
                }

                import.Status = status;
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
            finally
            {
                await cloudBlockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }

            return true;
        }
        
        public async Task<IStatus> GetStatus(string releaseId, string dataFileName)
        {
            var import = await GetImport(releaseId, dataFileName);
            return import.Status;
        }

        public async Task FailImport(string releaseId, string dataFileName, List<string> errors)
        {
            var import = await GetImport(releaseId, dataFileName);
            if (import.Status != IStatus.COMPLETE)
            {
                import.Status = IStatus.FAILED;
                import.Errors = JsonConvert.SerializeObject(errors);
                await _table.ExecuteAsync(TableOperation.InsertOrReplace(import));
            }
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
            return new BitArray(import.BatchesProcessed).Get(batchNo - 1);
        }

        public async Task CreateImport(string releaseId, string dataFileName, int numberOfRows, int numBatches, ImportMessage message)
        {
            await _table.ExecuteAsync(TableOperation.InsertOrReplace(
                new DatafileImport(releaseId, dataFileName, numberOfRows, numBatches, JsonConvert.SerializeObject(message)))
            );
        }

        private async Task<DatafileImport> GetImport(string releaseId, string dataFileName)
        {
            var result = await _table.ExecuteAsync(TableOperation.Retrieve<DatafileImport>(
                releaseId,
                dataFileName,
                new List<string> {"NumBatches", "BatchesProcessed", "Status", "NumberOfRows", "Errors", "Message"}));

            return (DatafileImport) result.Result;
        }

        private async Task<string> GetLease(CloudBlockBlob cloudBlockBlob)
        {
            string leaseId = null;

            // TODO Improve error handling & max retries 
            while (leaseId == null)
                try
                {
                    leaseId = await _fileStorageService.GetLeaseId(cloudBlockBlob);
                }
                catch (StorageException se)
                {
                    if (se.RequestInformation.HttpStatusCode == (int) HttpStatusCode.Conflict)
                        // A Conflict has been found, lease is being used by another process
                        // wait and try again.
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        throw se;
                    }
                }

            return leaseId;
        }
    }
}